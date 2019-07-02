using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Bloom.WebLibraryIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomHarvester.WebLibraryIntegration
{
	internal class HarvesterS3Client : BloomS3Client
	{
		public const string HarvesterUnitTestBucketName = "bloomharvest-unittests";
		public const string HarvesterSandboxBucketName = "bloomharvest-sandbox";
		public const string HarvesterProductionBucketName = "bloomharvest";

		public HarvesterS3Client(string bucketName)
			: base(bucketName)
		{
		}

		internal static IEnumerable<string> EnumerateAllBloomLibraryBucketNames()
		{
			yield return ProductionBucketName;
			yield return SandboxBucketName;
			yield return UnitTestBucketName;
		}

		internal static IEnumerable<string> EnumerateAllHarvesterBucketNames()
		{
			yield return HarvesterProductionBucketName;
			yield return HarvesterSandboxBucketName;
			yield return HarvesterUnitTestBucketName;
		}

		protected override IAmazonS3 CreateAmazonS3Client(string bucketName, AmazonS3Config s3Config)
		{
			return new AmazonS3Client(
				Environment.GetEnvironmentVariable("BloomHarvesterS3Key"),
				Environment.GetEnvironmentVariable("BloomHarvesterS3SecretKey"),
				RegionEndpoint.USEast1);
		}

		/// <summary>
		/// Uploads a single file to AWS S3
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="uploadFolderKey">The key prefix of the S3 object to create (i.e., which subfolder to upload it to)</param>
		public void UploadFile(string filePath, string uploadFolderKey)
		{
			using (var fileTransferUtility = new TransferUtility(GetAmazonS3(_bucketName)))
			{
				fileTransferUtility.Upload(filePath, $"{_bucketName}/{uploadFolderKey}");
			}
		}

		/// <summary>
		/// Uploads a directory to AWS S3
		/// </summary>
		/// <param name="directoryToUpload">The local directory whose contents should be uploaded to S3</param>
		/// <param name="uploadFolderKey">The key prefix of the S3 objects to create (i.e., which subfolder to upload it to)</param>
		public void UploadDirectory(string directoryToUpload, string uploadFolderKey)
		{
			// Delete the directory first in case the directory to upload is a strict subset of the existing contents.
			DeleteBookData(_bucketName, uploadFolderKey);

			using (var transferUtility = new TransferUtility(GetAmazonS3(_bucketName)))
			{
				var request = new TransferUtilityUploadDirectoryRequest
				{
					Directory = directoryToUpload,
					BucketName = _bucketName,
					KeyPrefix = uploadFolderKey,
					SearchPattern = "*.*",
					SearchOption = System.IO.SearchOption.AllDirectories,
					CannedACL = S3CannedACL.PublicRead
				};

				// Enhance: Could call the BloomDesktop code directly in future if desired.
				transferUtility.UploadDirectory(request);
			}
		}
	}
}
