using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace services.S3.Abstractions
{
    public interface IS3ClientProvider
    {
        public S3ClientDTO GetS3Client();
    }
}
