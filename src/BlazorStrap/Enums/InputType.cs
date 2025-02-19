﻿using System.ComponentModel;

namespace BlazorStrap
{
    public enum InputType
    {
        [Description("checkbox")]
        Checkbox,
        [Description("color")]
        Color,
        [Description("date")]
        Date,
        [Description("datetime-local")]
        DateTimeLocal,
        [Description("email")]
        Email,
        [Description("file")]
        File,
        [Description("month")]
        Month,
        [Description("number")]
        Number,
        [Description("text")]
        Money,
        [Description("text")]
        Percent,
        [Description("password")]
        Password,
        [Description("radio")]
        Radio,
        [Description("range")]
        Range,
        [Description("search")]
        Search,
        [Description("select")]
        Select,
        [Description("tel")]
        Tel,
        [Description("text")]
        Text,
        [Description("text")]
        Mask,
        [Description("textarea")]
        TextArea,
        [Description("time")]
        Time,
        [Description("url")]
        Url,
        [Description("week")]
        Week
    }
}
