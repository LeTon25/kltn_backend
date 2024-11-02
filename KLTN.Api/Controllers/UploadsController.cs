using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KLTN.Application.Helpers.Response;
using KLTN.Application.DTOs.Uploads;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace KLTN.Api.Controllers
{
    public class UploadsController : BaseController
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string? _bucketName;
        public UploadsController(IAmazonS3 _s3Client, IConfiguration configuration) 
        {
            this._s3Client = _s3Client;
            this._bucketName = configuration["AWS:BucketName"];
        }
        [HttpPost("s3/multiple")]
        public async Task<IActionResult> UploadMultipleFilesAsync([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return Ok(new ApiResponse<string>(200,"Không có file để upload"));
            }
            var uploadResults = new List<FileDto>();
            foreach (var file in files)
            {
                var filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var s3Url = await UploadFileAsync(filePath, file.FileName);
                var fileType = file.ContentType;

                uploadResults.Add(new FileDto(s3Url, file.FileName,fileType));
            }
            return Ok(new ApiResponse<List<FileDto>>(200,"Thêm thành công",uploadResults));
        }
        [HttpDelete("s3/mutiple")]
        public async Task<IActionResult> DeleteMultipleFilesAsync(string[] fileNames)
        {
            if (fileNames == null || fileNames.Count() == 0)
            {
                return Ok(new ApiResponse<string>(200, "Không có file để xóa"));
            }
            foreach(var fileName in fileNames)
            {
                await DeleteFilesAsync(fileName);
            }  
            return Ok(new ApiResponse<string>(200, "Xóa thành công"));
        }
        [HttpGet("s3/files")]
        public async Task<IActionResult> GetFilesInAwsAsync()
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName
            };

            var response = await _s3Client.ListObjectsV2Async(request);

            var files = response.S3Objects.Select(o => new FileDto( 
                 $"https://{_bucketName}.s3.amazonaws.com/{o.Key}",
                 o.Key,
                 o.Key.Split('.').Last()
                 )
            ).ToList();

            return Ok(new ApiResponse<List<FileDto>>(200, "Thành công", files));

        }
        private async Task<string> UploadFileAsync(string filePath, string keyName)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            await fileTransferUtility.UploadAsync(filePath,_bucketName, keyName);

            // Trả về URL của file
            return $"https://{_bucketName}.s3.amazonaws.com/{keyName}";
        }
        private async Task<bool> DeleteFilesAsync(string keyName)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
