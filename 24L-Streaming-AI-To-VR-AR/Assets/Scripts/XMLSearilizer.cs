using System;
using System.IO;
using System.Xml.Serialization;

class XMLSerializer
{
    CurrentWeather currentWeather;
    XMLShipStructure shipInformation;
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

    public static string WriteToXmlStringWeather(CurrentWeather currentWeather)
    {
        using (StringWriter writer = new StringWriter())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CurrentWeather));
            serializer.Serialize(writer, currentWeather);
            return writer.ToString();
        }
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
        using (StringWriter writer = new StringWriter())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLShipStructure));
            serializer.Serialize(writer, shipInformation);
            return writer.ToString();
        }
    }
}