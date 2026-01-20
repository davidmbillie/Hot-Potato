using System;
using System.Collections.Generic;
using System.Text;

namespace HotPotato.ChatClient.Models
{
	public class AzureOpenAIOptions
	{
		public string Endpoint { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;
		public string Deployment { get; set; } = string.Empty;
	}
}
