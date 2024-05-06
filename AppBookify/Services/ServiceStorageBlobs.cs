using Azure.Storage.Blobs;

namespace AppBookify.Services
{
    public class ServiceStorageBlobs
    {
        private BlobServiceClient client;
        public ServiceStorageBlobs(BlobServiceClient client)
        {
            this.client = client;
        }

        //METODO PARA MOSTRAR LOS CONTAINERS 
        //public async Task<List<string>> GetContainersAsync()
        //{
        //    List<string> containers = new List<string>();

        //    await foreach (BlobContainerItem item in this.client.GetBlobContainersAsync())
        //    {
        //        containers.Add(item.Name);
        //    }
        //    return containers;
        //}

        //METODO PARA CREAR UN CONTENEDOR 
        //public async Task CreateContainerAsync(string containerName)
        //{
        //    await this.client.CreateBlobContainerAsync

        //        (containerName, PublicAccessType.Blob);
        //}

        //METODO PARA SUBIR UN BLOB A UN CONTAINER 
        public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
        {
            BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            //await containerClient.UploadBlobAsync(blobName, stream);
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        //METODO PARA ELIMINAR UN BLOB DE UN CONTAINER 
        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient =
                this.client.GetBlobContainerClient(containerName);

            await containerClient.DeleteBlobAsync(blobName);
        }

        public async Task<BlobClient> FindBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                return blobClient;
            }
            else
            {
                return null;
            }
        }

        //METODO PARA ELIMINAR UN CONTENEDOR 
        public async Task DeleteContainerAsync(string containerName)
        {
            await this.client.DeleteBlobContainerAsync(containerName);
        }
    }
}
