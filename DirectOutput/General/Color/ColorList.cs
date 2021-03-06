﻿using System.Xml;
using System.Xml.Serialization;
using DirectOutput.General.Generic;
using DirectOutput.Cab.Toys.Layer;

namespace DirectOutput.General.Color
{

    /// <summary>
    /// List for Color objects 
    /// </summary>
    public class ColorList : NamedItemList<RGBAColorNamed>, IXmlSerializable
    {

        #region IXmlSerializable implementation
        /// <summary>
        /// Serializes the Color objects in this list to Xml.<br/>
        /// WriteXml is part of the IXmlSerializable interface.
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {

            XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces();
            Namespaces.Add(string.Empty, string.Empty);
            foreach (RGBAColorNamed C in this)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RGBAColorNamed));
                serializer.Serialize(writer, C, Namespaces);
            }
        }


        /// <summary>
        /// Deserializes the Color objects in the XmlReader.<br/>
        /// ReadXml is part of the IXmlSerializable interface.
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                return;
            }

            reader.Read();

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.LocalName == typeof(RGBAColorNamed).Name)
                {

                    XmlSerializer serializer = new XmlSerializer(typeof(RGBAColorNamed));
                    RGBAColorNamed C = (RGBAColorNamed)serializer.Deserialize(reader);
                    if (!Contains(C.Name))
                    {
                        Add(C);
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
            reader.ReadEndElement();
        }


        /// <summary>
        /// Method is required by the IXmlSerializable interface
        /// </summary>
        /// <returns>Returns always null</returns>
        public System.Xml.Schema.XmlSchema GetSchema() { return (null); }
        #endregion


    }
}
