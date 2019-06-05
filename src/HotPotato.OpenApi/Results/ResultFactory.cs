﻿using HotPotato.OpenApi.Models;
using HotPotato.OpenApi.Validators;
using System.Linq;

namespace HotPotato.OpenApi.Results
{
    public static class ResultFactory
    {
        public static Result PassResult(string path, string method, int statusCode) =>
            new PassResult(path, method, statusCode);
        public static Result FailResult(string path, string method, int statusCode, Reason[] reasons, params ValidationError[] validationErrors) =>
            new FailResult(path, method, statusCode, reasons?.ToList(), validationErrors?.ToList());
    }
}
