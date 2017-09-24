using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hunt.Mobile.Common
{
	public class StorageService
	{
		public async Task<string> SaveImage(byte[] image, string gameId, string treasureId = null)
		{
			try
			{
				var blobId = Guid.NewGuid().ToString() + ".jpg";
				var task = new Task<string>(() => App.Instance.DataService.GetStorageToken(blobId).Result);
				await task.RunProtected();

				if(task.IsFaulted)
					return null;

				var token = task.Result;
				var uri = new Uri(token);

				var blockBlob = new CloudBlockBlob(uri);
				blockBlob.Properties.ContentType = "image/jpg";
				blockBlob.Metadata.Add("gameId", gameId);

				var uploadTask = new Task(() => blockBlob.UploadFromByteArrayAsync(image, 0, image.Length).Wait());
				await uploadTask.RunProtected();
				return blockBlob.StorageUri.PrimaryUri.ToString();
			}
			catch(Exception e)
			{
				Logger.Instance.WriteLine(e);
			}

			return null;
		}
	}
}