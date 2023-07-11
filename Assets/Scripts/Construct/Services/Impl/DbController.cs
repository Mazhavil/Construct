using System.IO;
using UnityEngine;

namespace Construct.Services.Impl
{
    /// <summary>
    /// Контроллер взаимодействия с удаленной базой данных.
    /// </summary>
    public sealed class DbController : IDbController
    {
        private readonly string _path = "hirearchy";

        public ConventusDto DonwloadConventus(int id)
        {
            var fileContent = Resources.Load<TextAsset>(_path);
            return JsonUtility.FromJson<ConventusDto>(fileContent.text);
        }
    }
}
