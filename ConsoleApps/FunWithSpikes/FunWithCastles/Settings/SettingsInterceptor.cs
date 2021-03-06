﻿using Castle.DynamicProxy;

namespace FunWithCastles.Settings
{
    public class SettingsInterceptor : IInterceptor
    {
        private readonly ISettingsAdapter _adapter;
        private readonly ISettingConverter _converter;

        public SettingsInterceptor(ISettingsAdapter adapter, ISettingConverter converter)
        {
            _adapter = adapter;
            _converter = converter;
        }

        public void Intercept(IInvocation invocation)
        {
            var name = invocation.Method.Name;
            bool handled = false;
            if (name.StartsWith("get_"))
            {
                handled = Get(invocation, name);
            }
            else if (name.StartsWith("set_"))
            {
                handled = Set(invocation, name);
            }

            if (!handled)
            {
                invocation.Proceed();
            }
        }

        private static string Clean(string input, string prefix)
        {
            return input.StartsWith(prefix)
                ? input.Substring(prefix.Length)
                : input;
        }

        private bool Get(IInvocation invocation, string name)
        {
            name = Clean(name, "get_");
            object adapterValue;
            if (_adapter.TryRead(name, out adapterValue))
            {
                var convertedValue = _converter.ConvertTo(invocation.Method.ReturnType, adapterValue);
                invocation.ReturnValue = convertedValue;
                return true;
            }

            return false;
        }

        private bool Set(IInvocation invocation, string name)
        {
            name = Clean(name, "set_");
            var value = invocation.Arguments[0];
            return _adapter.TryWrite(name, value);
        }
    }
}