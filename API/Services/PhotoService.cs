using System;
using System.Threading.Tasks;
using API.Helper;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IOptions<CloudinarySettings> _cloudinarySettings;
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings;
            var account = new Account
                ( _cloudinarySettings.Value.CloudName,
                  _cloudinarySettings.Value.ApiKey,
                  _cloudinarySettings.Value.ApiSecret
                );
            _cloudinary = new Cloudinary(account);
        }
        /// <inheritdoc />
        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
          var uploadResult = new ImageUploadResult();
          if (file.Length <= 0) return uploadResult;
          await using var stream = file.OpenReadStream();
          var uploadParameters = new ImageUploadParams()
          {
              File = new FileDescription(file.FileName, stream),
              Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
          };

          uploadResult = await _cloudinary.UploadAsync(uploadParameters);

          return uploadResult;
        }

        /// <inheritdoc />
        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result;
        }
    }
}
