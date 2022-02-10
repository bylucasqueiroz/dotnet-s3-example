using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Connect
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the bucket name: ");
            var bucketName = Console.ReadLine();

            try
            {
                var s3Client = new AmazonS3Client();

                var awsS3Response = await ListAndPrintBuckets(s3Client);
                var isABucketWithThatName = awsS3Response.Buckets.Where(x => x.BucketName == bucketName).Count() > 0;
                 
                if (!string.IsNullOrEmpty(bucketName) && isABucketWithThatName)
                {
                    await CreateBucket(s3Client, bucketName);
                }

                var pathInLocalMachine = @"../../../Files/text.txt";

                string destPath = await PutFileToBucket(bucketName, s3Client, pathInLocalMachine);

                await s3Client.DeleteObjectAsync(bucketName, destPath);
                await s3Client.DeleteBucketAsync(bucketName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task<string> PutFileToBucket(string bucket, AmazonS3Client s3Client, string path)
        {
            FileInfo localFile = new FileInfo(path);

            var destPath = $"files/data-{DateTime.Now.ToFileTimeUtc()}.txt";

            PutObjectRequest request = new PutObjectRequest()
            {
                InputStream = localFile.OpenRead(),
                BucketName = bucket,
                Key = destPath,
            };

            if (request.InputStream != null)
            {
                PutObjectResponse response = await s3Client.PutObjectAsync(request);
                Console.WriteLine($"Result: {response.HttpStatusCode}");
            }

            return destPath;
        }

        private static async Task<ListBucketsResponse> ListAndPrintBuckets(AmazonS3Client s3Client)
        {
            var listResponse = await MyListBucketsAsync(s3Client);

            Console.WriteLine($"Number of buckets: {listResponse.Buckets.Count}");

            foreach (S3Bucket b in listResponse.Buckets)
            {
                Console.WriteLine(b.BucketName);
            }

            return listResponse;
        }

        private static async Task CreateBucket(AmazonS3Client s3Client, string bucketName)
        {
            Console.WriteLine($"\nCreating bucket {bucketName}...");
            var createResponse = await s3Client.PutBucketAsync(bucketName);
            Console.WriteLine($"Result: {createResponse.HttpStatusCode}");
        }

        private static Boolean GetBucketName(string args, out String bucketName)
        {
            Boolean retval = false;
            bucketName = String.Empty;

            if (!string.IsNullOrEmpty(args))
            {
                bucketName = args;
                retval = true;
            }

            return retval;
        }

        private static async Task<ListBucketsResponse> MyListBucketsAsync(IAmazonS3 s3Client)
        {
            return await s3Client.ListBucketsAsync();
        }
    }
}
