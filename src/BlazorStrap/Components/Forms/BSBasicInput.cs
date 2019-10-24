﻿using BlazorComponentUtilities;
using BlazorStrap.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorStrap
{
   
    public class BSBasicInput<T> : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> UnknownParameters { get; set; }
        [CascadingParameter] protected EditContext MyEditContext { get; set; }
        [Inject] protected Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; }

        private const string _dateFormat = "yyyy-MM-dd";
        protected string Classname =>
        new CssBuilder()
           .AddClass($"form-control-{Size.ToDescriptionString()}", Size != Size.None)
           .AddClass("is-valid", IsValid)
           .AddClass("is-invalid", IsInvalid)

           .AddClass(GetClass())
           .AddClass(Class)
         .Build();

        protected string Tag => InputType switch
        {
            InputType.Select => "select",
            InputType.TextArea => "textarea",
            _ => "input"
        };

        private FieldIdentifier _fieldIdentifier { get; set; }

        [Parameter] public Expression<Func<object>> For { get; set; }
        [Parameter] public InputType InputType { get; set; } = InputType.Text;
        [Parameter] public Size Size { get; set; } = Size.None;
        [Parameter] public virtual T Value { get; set; }
        [Parameter] public virtual T RadioValue { get; set; }
        [Parameter] public virtual EventCallback<T> ValueChanged { get; set; }
        [Parameter] public EventCallback<string> ConversionError { get; set; }
        [Parameter] public bool IsReadonly { get; set; }
        [Parameter] public bool IsPlaintext { get; set; }
        [Parameter] public bool IsDisabled { get; set; }
        [Parameter] public bool IsChecked { get; set; }
        [Parameter] public bool IsValid { get; set; }
        [Parameter] public bool IsInvalid { get; set; }
        [Parameter] public bool IsMultipleSelect { get; set; }
        [Parameter] public int? SelectSize { get; set; }
        [Parameter] public int? SelectedIndex { get; set; }
        [Parameter] public string Class { get; set; }
        [Parameter] public CultureInfo CultureInfo { get; set; } = new CultureInfo("pt-BR");
        [Parameter] public string Mask { get; set; }
        [Parameter] public bool MaskReverse { get; set; }
        [Parameter] public string MaskPlaceholder { get; set; }

        // [Parameter] public string Class { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }

        protected string Type => InputType.ToDescriptionString();

        protected override void OnParametersSet()
        {
            if (For != null)
            {
                _fieldIdentifier = FieldIdentifier.Create(For);
            }
        }
        
        private string GetClass()
        {
            return InputType switch
            {
                InputType.Checkbox => "form-check-input",
                InputType.Radio => "form-check-input",
                InputType.File => "form-control-file",
                InputType.Range => "form-control-range",
                _ => IsPlaintext ? "form-control-plaintext" : "form-control"
            };
        }

        protected void OnChange(string e)
        {
            CurrentValueAsString = e;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (UnknownParameters.TryGetValue("id", out var id))
                {
                    var mask = InputType switch
                    {
                        InputType.Money => "0.000.000.000.000,00",
                        InputType.Percent => "000,00",
                        InputType.Mask => Mask,
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(mask))
                    {
                        var maskPlaceHolder = InputType switch
                        {
                            InputType.Money => "0.00",
                            InputType.Percent => "0.00",
                            InputType.Mask => MaskPlaceholder,
                            _ => ""
                        };

                        var maskReverse = InputType switch
                        {
                            InputType.Money => true,
                            InputType.Percent => true,
                            InputType.Mask => MaskReverse,
                            _ => false
                        };

                        await new BlazorStrapInterop(JSRuntime).SetMask(id.ToString(), mask, maskReverse, maskPlaceHolder);
                    }
                }
            }
        }

        protected void OnClick(MouseEventArgs e)
        {
            if (InputType == InputType.Radio)
            {
                Value = (T)(object)(RadioValue);
                ValueChanged.InvokeAsync(Value);
            }
            else
            { 
                var tmp = (bool)(object)Value;
                Value = (T)(object)(!tmp);
                ValueChanged.InvokeAsync(Value);
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder?.OpenElement(0, Tag);
            builder.AddMultipleAttributes(1, UnknownParameters);
            builder.AddAttribute(2, "class", Classname);
            builder.AddAttribute(3, "type", Type);
            builder.AddAttribute(4, "readonly", IsReadonly);
            builder.AddAttribute(5, "disabled", IsDisabled);
            builder.AddAttribute(6, "multiple", IsMultipleSelect);
            builder.AddAttribute(7, "size", SelectSize);
            builder.AddAttribute(8, "selectedIndex", SelectedIndex);
            if (InputType == InputType.Checkbox)
            {
                if(typeof(T) == typeof(string))
                {
                    Value = ((string)(object)Value).ToLowerInvariant() != "false" ? (T)(object)"true" : (T)(object)"false";
                }
                builder.AddAttribute(8, "checked", Convert.ToBoolean(Value, CultureInfo));
                builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, OnClick));
            }
            else if(InputType == InputType.Radio)
            {
                if (RadioValue != null)
                {
                    if (RadioValue.Equals(Value))
                    {
                        builder.AddAttribute(8, "checked", true);
                        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, OnClick));
                    }
                    else
                    {
                        builder.AddAttribute(8, "checked", false);
                        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, OnClick));
                    }
                }
            }
            else
            {
                builder.AddAttribute(8, "value", BindConverter.FormatValue(CurrentValueAsString));
                builder.AddAttribute(10, "onchange", EventCallback.Factory.CreateBinder<string>(this, OnChange, CurrentValueAsString));
            }
            builder.AddContent(10, ChildContent);
            builder.CloseElement();
        }

        public void ForceValidate()
        {
            MyEditContext?.Validate();
            StateHasChanged();
        }

        protected string FormatValueAsString(T value)
        {
            return value switch
            {
                null => null,
                int @int => BindConverter.FormatValue(@int, CultureInfo),
                long @long => BindConverter.FormatValue(@long, CultureInfo),
                float @float => BindConverter.FormatValue(@float, CultureInfo),
                double @double => BindConverter.FormatValue(@double, CultureInfo),
                decimal @decimal => InputType == InputType.Money || InputType == InputType.Percent ? @decimal.ToString("#,#.00###;(#,#.00###)", CultureInfo)
                                                                                                   : BindConverter.FormatValue(@decimal, CultureInfo),
                DateTime dateTimeValue => BindConverter.FormatValue(dateTimeValue, _dateFormat, CultureInfo),
                DateTimeOffset dateTimeOffsetValue => BindConverter.FormatValue(dateTimeOffsetValue, _dateFormat, CultureInfo),
                _ => value?.ToString()
            };
        }

        protected bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            Type type = typeof(T);
            if (typeof(T) == typeof(string))
            {
                result = (T)(object)value;
                validationErrorMessage = null;
                return true;
            }
            else if (value == null && (Nullable.GetUnderlyingType(type) != null))
            {
                result = (T)(object)default(T);
                validationErrorMessage = null;
                return true;
            }
            else if (value?.Length == 0 && typeof(DateTime) != typeof(T) && typeof(DateTimeOffset) != typeof(T))
            {
                result = (T)(object)default(T);
                validationErrorMessage = null;
                return true;
            }
            else if (typeof(T).IsEnum)
            {
                // There's no non-generic Enum.TryParse (https://github.com/dotnet/corefx/issues/692)
                try
                {
                    result = (T)Enum.Parse(typeof(T), value);
                    validationErrorMessage = null;
                    return true;
                }
                catch (ArgumentException)
                {
                    result = default;
                    validationErrorMessage = $"The {type.Name} field is not valid.";
                    return false;
                }
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                result = (T)(object)Convert.ToInt32(value, CultureInfo);
                validationErrorMessage = null;
                return true;
            }
            else if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
            {
                result = (T)(object)Convert.ToInt64(value, CultureInfo);
                validationErrorMessage = null;
                return true;
            }
            else if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
            {
                result = (T)(object)double.Parse(value, CultureInfo);
                validationErrorMessage = null;
                return true;
            }
            else if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
            {
                result = (T)(object)decimal.Parse(value, CultureInfo);
                validationErrorMessage = null;
                return true;
            }
            else if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
            {
                try
                {
                    result = (T)(object)Guid.Parse(value);
                    validationErrorMessage = null;
                }
                catch
                {
                    result = (T)(object) new Guid();
                    validationErrorMessage = "Invalid Guid format";
                }
                
                return true;
            }
            else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                if (TryParseDateTime(value, out result, CultureInfo))
                {
                    validationErrorMessage = null;
                    return true;
                }
                else
                {
                    validationErrorMessage = string.Format(CultureInfo, "The {0} field must be a date.", type.Name);
                    return false;
                }
            }
            else if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                if (TryParseDateTimeOffset(value, out result, CultureInfo))
                {
                    validationErrorMessage = null;
                    return true;
                }
                else
                {
                    validationErrorMessage = string.Format(CultureInfo, "The {0} field must be a date.", type.Name);
                    return false;
                }
            }
            throw new InvalidOperationException($"{GetType()} does not support the type '{typeof(T)}'.");
        }

        private static bool TryParseDateTime(string value, out T result, CultureInfo cultureInfo)
        {
            var success = BindConverter.TryConvertToDateTime(value, cultureInfo, _dateFormat, out DateTime parsedValue);
            if (success)
            {
                result = (T)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        private static bool TryParseDateTimeOffset(string value, out T result, CultureInfo cultureInfo)
        {
            var success = BindConverter.TryConvertToDateTimeOffset(value, cultureInfo, _dateFormat, out DateTimeOffset parsedValue);
            if (success)
            {
                result = (T)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        protected string CurrentValueAsString
        {
            get => FormatValueAsString(CurrentValue);
            set
            {
                _ = TryParseValueFromString(value, out T parsedValue, out var validationErrorMessage);
                CurrentValue = parsedValue;
                _ = ConversionError.InvokeAsync(validationErrorMessage);
            }
        }

        protected T CurrentValue
        {
            get => Value;
            set
            {
                var hasChanged = !EqualityComparer<T>.Default.Equals(value, Value);
                if (hasChanged)
                {
                    Value = value;
                    _ = ValueChanged.InvokeAsync(value);
                }
            }
        }
    }
}
