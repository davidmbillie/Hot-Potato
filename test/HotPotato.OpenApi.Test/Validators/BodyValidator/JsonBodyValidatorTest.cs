using HotPotato.OpenApi.Models;
using NJsonSchema;
using System.Threading.Tasks;
using Xunit;

namespace HotPotato.OpenApi.Validators
{
	public class JsonBodyValidatorTest
	{
		private const string AValidBody = "{'foo': 1}";
		private const string AValidSchema = @"{'properties':{'foo':{'type':'integer'}}}";

		private const string AnInvalidBody = "{'foo': 'abc'}";
		private const string ABodyWithAnUnexpectedProperty = "{'bar': 2}";

		private const string AValidNullableBody = "{'foo': null}";
		//nullable in yaml converts to x-nullable in json
		private const string AValidNullableSchema = @"{'properties':{'foo':{'type':'integer','x-nullable':true}}}";

		[Fact]
		public async Task JsonBodyValidator_ReturnsTrueWithValid()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidSchema);
			JsonBodyValidator subject = new JsonBodyValidator(AValidBody);

			IValidationResult result = subject.Validate(schema);

			Assert.True(result.Valid);
		}

		[Fact]
		public async Task JsonBodyValidator_ReturnsFalseWithInvalid()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidSchema);
			JsonBodyValidator subject = new JsonBodyValidator(AnInvalidBody);

			InvalidResult result = (InvalidResult)subject.Validate(schema);

			Assert.False(result.Valid);
			Assert.Equal(Reason.InvalidBody, result.Reason);
			Assert.Equal(ValidationErrorKind.IntegerExpected, result.Errors[0].Kind);
		}

		[Fact]
		public async Task JsonBodyValidator_ReturnsFalseWithUndocumentedProperty()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidSchema);
			JsonBodyValidator subject = new JsonBodyValidator(ABodyWithAnUnexpectedProperty);

			InvalidResult result = (InvalidResult)subject.Validate(schema);

			Assert.False(result.Valid);
			Assert.Equal(Reason.InvalidBody, result.Reason);
			Assert.Equal(ValidationErrorKind.PropertyNotInSpec, result.Errors[0].Kind);
		}

		[Fact]
		public void JsonBodyValidator_ReturnsFalseWithUndocumentedPropertyAndBlankSchema()
		{
			JsonSchema schema = new JsonSchema();
			JsonBodyValidator subject = new JsonBodyValidator(ABodyWithAnUnexpectedProperty);

			InvalidResult result = (InvalidResult)subject.Validate(schema);

			Assert.False(result.Valid);
			Assert.Equal(Reason.InvalidBody, result.Reason);
			Assert.Equal(ValidationErrorKind.PropertyNotInSpec, result.Errors[0].Kind);
		}

		//the cases for null body and null schema will now be addressed by the ContentValidator

		[Fact]
		public async Task JsonBodyValidator_ReturnsTrueWithValidNullable()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidNullableSchema);
			JsonBodyValidator subject = new JsonBodyValidator(AValidNullableBody);

			IValidationResult result = subject.Validate(schema);

			Assert.True(result.Valid);
		}

		[Fact]
		public async Task JsonBodyValidator_ReturnsFalseWithUnexpectedNullable()
		{
			JsonSchema schema = await JsonSchema.FromJsonAsync(AValidSchema);
			JsonBodyValidator subject = new JsonBodyValidator(AValidNullableBody);

			InvalidResult result = (InvalidResult)subject.Validate(schema);

			Assert.False(result.Valid);
			Assert.Equal(Reason.InvalidBody, result.Reason);
			Assert.Equal(ValidationErrorKind.IntegerExpected, result.Errors[0].Kind);
		}
	}
}
