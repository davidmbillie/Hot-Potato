﻿
using HotPotato.OpenApi.Models;
using HotPotato.OpenApi.Results;
using HotPotato.OpenApi.SpecificationProvider;
using NSwag;

namespace HotPotato.OpenApi.Validators
{
    public class ValidationStrategy : IValidationStrategy
    {
        internal PathValidator PathValidator { get; set; }
        internal MethodValidator MethodValidator { get; set; }
        internal StatusCodeValidator StatusCodeValidator { get; set; }
        internal BodyValidator BodyValidator { get; set; }
        internal HeaderValidator HeaderValidator { get; set; }

        private IResultCollector resColl { get; }
        private SwaggerDocument swagDoc { get; }

        public ValidationStrategy(IResultCollector ResColl, ISpecificationProvider SpecPro)
        {
            resColl = ResColl;
            swagDoc = SpecPro.GetSpecDocument();
        }

        public void Validate()
        {
            if (!PathValidator.Validate(swagDoc))
            {
                AddFail(Reason.MissingPath);
                return;
            }
            if(!MethodValidator.Validate(PathValidator.Result))
            {
                AddFail(Reason.MissingMethod);
                return;
            }
            if(!StatusCodeValidator.Validate(MethodValidator.Result))
            {
                AddFail(StatusCodeValidator.FailReason);
                return;
            }
            else if(StatusCodeValidator.statCode == 204)
            {
                AddPass();
                return;
            }

            if (!BodyValidator.Validate(StatusCodeValidator.Result))
            {
                AddFail(BodyValidator.FailReason, BodyValidator.ErrorArr);
            }
            else
            {
                AddPass();
            }
            if (!HeaderValidator.Validate(StatusCodeValidator.Result))
            {
                AddFail(HeaderValidator.FailReason, HeaderValidator.ErrorArr);
            }
            else
            {
                AddPass();
            }
        }

        public void AddFail(Reason reason, params ValidationError[] validationErrors)
        {
            resColl.Fail(PathValidator.path, MethodValidator.method, StatusCodeValidator.statCode, reason, validationErrors);
        }
        
        public void AddPass()
        {
            resColl.Pass(PathValidator.path, MethodValidator.method, StatusCodeValidator.statCode);
        }
    }
}
