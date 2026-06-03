using System.Collections.Generic;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Services.MultipartForm
{
    /// <summary>
    /// Legacy extension point for objects that add their own multipart form sections.
    /// </summary>
    public interface IMultiFormPropertyAdder
    {
        /// <summary>
        /// Adds this object's form sections to the outgoing multipart request.
        /// </summary>
        /// <param name="form">Mutable multipart form section list.</param>
        public void AddPropertiesToForm(List<IMultipartFormSection> form);
    }
}
