using HotPotato.OpenApi.Validators;
using NJsonSchema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HotPotato.OpenApi.Filters
{
	public class NullableValidationErrorFilterTest
	{
		private const string AValidSchema = @"{'type': 'integer'}";

		private const string AValidNullableBody = "{'foo': null}";
		//nullable in yaml converts to x-nullable in json
		private const string AValidNullableSchema = @"{'properties':{'foo':{'type':'integer','x-nullable':true}}}";
		private const string AValidJsonArray = @"[{'testsPassed': 12,'testsTotal': 12,'id': 4483,'url': 'TestMetricsURL'},{'branchesCovered': 4,'branchesTotal': 4,'linesCovered': 8,'linesTotal': 8,'id': 4483,'url': null},{'id': 4483,'url': 'PerformanceMetricsURL'}]";

		[Fact]
		public async Task NullableValidationErrorFilter_Filter_RemovesFalseNullErrors()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidNullableSchema);

			ICollection<NJsonSchema.Validation.ValidationError> errors = schema.Validate(AValidNullableBody);
			List<ValidationError> errList = errors.ToValidationErrorList();

			NullableValidationErrorFilter filter = new NullableValidationErrorFilter(schema, AValidNullableBody);

			filter.Filter(errList);

			Assert.Empty(errList);
		}

		[Fact]
		public async Task NullableValidationErrorFilter_Filter_DoesNotRemoveTrueErrors()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidSchema);

			ICollection<NJsonSchema.Validation.ValidationError> errors = schema.Validate(AValidNullableBody);
			List<ValidationError> errList = errors.ToValidationErrorList();

			NullableValidationErrorFilter filter = new NullableValidationErrorFilter(schema, AValidNullableBody);

			filter.Filter(errList);

			Assert.Single(errList);
			Assert.Contains("IntegerExpected", errList.ElementAt(0).Message);

		}

		[Fact]
		public async Task NullableValidationErrorFilter_Filter_DoesNotThrowExceptionWithJArray()
		{
			JsonSchema schema = JsonSchema.CreateAnySchema();

			List<ValidationError> errList = new List<ValidationError>();

			NullableValidationErrorFilter filter = new NullableValidationErrorFilter(schema, AValidJsonArray);

			filter.Filter(errList);

			Assert.Empty(errList);
		}
	}
}
