
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace HotPotato.Core.Http.Default
{
	public class HotPotatoResponse : IHotPotatoResponse
	{
		public HotPotatoResponse(HttpStatusCode statusCode, HttpHeaders headers)
		{
			this.StatusCode = statusCode;
			this.Headers = headers;
		}
		public HotPotatoResponse(HttpStatusCode statusCode, HttpHeaders headers, byte[] content, string mediaType)
			: this(statusCode, headers)
		{
			this.Content = content;
			this.ContentType = new HttpContentType(mediaType);
		}
		public HotPotatoResponse(HttpStatusCode statusCode, HttpHeaders headers, byte[] content, MediaTypeHeaderValue contentType)
			: this(statusCode, headers)
		{
			this.Content = content;
			this.ContentType = new HttpContentType(contentType.MediaType, contentType.CharSet);
		}
		public HttpStatusCode StatusCode { get; }

		public HttpHeaders Headers { get; }

		public byte[] Content { get; }

		public HttpContentType ContentType { get; }

	}
}
