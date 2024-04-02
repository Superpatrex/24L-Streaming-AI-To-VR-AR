using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using UnityEngine;

/// <summary>
/// The XMLSerializer class is used to read and write XML files
/// </summary>
public class XMLSerializer
{

    /// <summary>
    /// Reads the XML string and returns the CurrentWeather object
    /// </summary>
    /// <param name="xmlString">The XML file string that is being read</param>
    /// <returns>The corresponding CurrentWeather object</returns>
    public static CurrentWeather ReadFromXmlStringWeather(string xmlString)
    {
        CurrentWeather currentWeather;

        using (StringReader reader = new StringReader(xmlString))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CurrentWeather));
            currentWeather = (CurrentWeather)serializer.Deserialize(reader);
        }

        return currentWeather;
    }

    /// <summary>
    /// Reads the XML string and returns the XMLShipStructure object
    /// </summary>
    /// <param name="xmlString">The XML file string that is being read in</param>
    /// <returns>The corresponding XMLShipStructure</returns>
    public static XMLShipStructure ReadFromXmlStringShipInformation(string xmlString)
    {
        XMLShipStructure shipInformation;

        using (StringReader reader = new StringReader(xmlString))
        {
            //Debug.Log("Starting xml deserialization");
            XmlSerializer serializer = new XmlSerializer(typeof(XMLShipStructure));
            //Debug.Log("Middle " + xmlString);
            shipInformation = (XMLShipStructure)serializer.Deserialize(reader);
            //Debug.Log("Ending xml deserialization");
        }

        return shipInformation;
    }

    /// <summary>
    /// Transcribes the XMLShipStructure object into an XML string that can be stores or used
    /// </summary>
    /// <param name="shipInformation">The corresponding XMLShipStructure object that is going to be serialized/param>
    /// <returns>The XML data that was transcribed from the shipInformation</returns>
    public static string WriteToXmlStringShipInformation(XMLShipStructure shipInformation)
    {
        var xmlSerializer = new XmlSerializer(typeof(XMLShipStructure));
        var ns = new XmlSerializerNamespaces();
        ns.Add("", ""); // This line removes the namespace declarations

        var utf8WithoutBom = new UTF8Encoding(false); // Create a UTF8Encoding without a BOM

        using (var memoryStream = new MemoryStream())
        using (var streamWriter = new StreamWriter(memoryStream, utf8WithoutBom))
        {
            xmlSerializer.Serialize(streamWriter, shipInformation, ns);
            var xmlString = Encoding.UTF8.GetString(memoryStream.ToArray()); // This will return the XML string with UTF-8 encoding

            // Replace lowercase encoding with uppercase and remove newline and indentation characters
            xmlString = xmlString.Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"").Replace("\r\n", "").Replace("  ", "");

            return xmlString.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "");
        }
    }

    /// <summary>
    /// Converts a UTF-8 string to a UTF-16 string
    /// </summary>
    /// <param name="utf16String">The UTF-16 string that is going to be converted</param>
    /// <returns>The UTF-8 string that was converted</returns>
    public static string ConvertUtf16ToUtf8(string utf16String)
    {
        byte[] utf16Bytes = Convert.FromBase64String(utf16String);
        byte[] utf8Bytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.UTF8, utf16Bytes);
        return System.Text.Encoding.UTF8.GetString(utf8Bytes);
    }
}