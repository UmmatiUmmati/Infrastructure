namespace Ummati.Infrastructure;

using System;
using System.Xml;

public static class DateTimeExtensions
{
    public static string ToRFC3339String(this DateTime value) =>
        XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc);
}
