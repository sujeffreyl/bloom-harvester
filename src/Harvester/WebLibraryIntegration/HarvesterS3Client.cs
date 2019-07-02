using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Bloom.WebLibraryIntegration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using L10NSharp;

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
				// This uploads but does not set public read
				// fileTransferUtility.Upload(filePath, $"{_bucketName}/{uploadFolderKey}");
				var request = new TransferUtilityUploadRequest()
				{
					BucketName = _bucketName,
					FilePath = filePath,
					Key = uploadFolderKey + "/" + Path.GetFileName(filePath)
				};
				// The effect of this is that navigating to the file's URL is always treated as an attempt to download the file.
				// This is definitely not desirable for the PDF (typically a preview) which we want to navigate to in the Preview button
				// of BloomLibrary.
				// I'm not sure whether there is still any reason to do it for other files. This code was copied from Bloom.
				// It was temporarily important for the BookOrder file when the Open In Bloom button just downloaded it.
				// However, now the download link uses the bloom: prefix to get the URL passed directly to Bloom,
				// it may not be needed for anything. Still, at least for the files a browser would not know how to
				// open, it seems desirable to download them with their original names, if such a thing should ever happen.
				// So I'm leaving the code in for now except in cases where we know we don't want it.
				// It is possible to also set the filename ( after attachment, put ; filename='" + Path.GetFileName(file) + "').
				// In principle this would be a good thing, since the massive AWS filenames are not useful.
				// However, AWSSDK can't cope with setting this for files with non-ascii names.
				// It seems that the header we insert here eventually becomes a header for a web request, and these allow only ascii.
				// There may be some way to encode non-ascii filenames to get the effect, if we ever want it again. Or AWS may fix the problem.
				// If you put setting the filename back in without such a workaround, be sure to test with a non-ascii book title.
				if (Path.GetExtension(filePath).ToLowerInvariant() != ".pdf")
					request.Headers.ContentDisposition = "attachment";
				request.CannedACL = S3CannedACL.PublicRead; // Allows any browser to download it.

				try
				{
					fileTransferUtility.Upload(request);

				}
				catch (Exception e)
				{
					throw;
				}
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
