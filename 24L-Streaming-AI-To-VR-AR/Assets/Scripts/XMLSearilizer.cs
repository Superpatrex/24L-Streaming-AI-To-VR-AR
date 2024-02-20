using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;

public class XMLSerializer
{
    public CurrentWeather currentWeather;
    public XMLShipStructure shipInformation;
    public XMLHolder holder;

    public void ReadFromXMLHolderShipInformation()
    {
        shipInformation = ReadFromXmlStringShipInformation(holder.shipInformation.text);
    }

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

    public static XMLShipStructure ReadFromXmlStringShipInformation(string xmlString)
    {
        XMLShipStructure shipInformation;

        using (StringReader reader = new StringReader(xmlString))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLShipStructure));
            shipInformation = (XMLShipStructure)serializer.Deserialize(reader);
        }

        return shipInformation;
    }

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

    public static string ConvertUtf16ToUtf8(string utf16String)
    {
        byte[] utf16Bytes = Convert.FromBase64String(utf16String);
        byte[] utf8Bytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.UTF8, utf16Bytes);
        return System.Text.Encoding.UTF8.GetString(utf8Bytes);
    }
}