using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Refactorius.Collections
{
    /// <summary>Xml serializable dictionary object.</summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
    /// <remarks>Boldly stolen from http://msdnrss.thecoderblogs.com/2008/05/06/xml-serializable-dictionary/ </remarks>
    [Serializable]
    public class XmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
        where TKey: notnull
    {
        #region XML tag names.

        private const string DICTIONARY = "dictionary";
        private const string ITEM = "item";
        private const string KEY = "key";
        private const string VALUE = "value";

        #endregion

        #region constructors

        /// <summary>Initializes a new instance of the <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"/> class.</summary>
        public XmlSerializableDictionary()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"/> class.</summary>
        /// <param name="info">A <see cref="System.Runtime.Serialization.SerializationInfo"/> object containing the information
        /// required to serialize the <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"/>.</param>
        /// <param name="context">A <see cref="System.Runtime.Serialization.StreamingContext"/> structure containing the source and
        /// destination of the serialized stream associated with the
        /// <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"></see>.</param>
        protected XmlSerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region static serialization methods

        /// <summary>Loads a <see cref="IDictionary&lt;TKey, TValue&gt;"/> from its XML representation.</summary>
        /// <param name="reader">The <see cref="XmlReader"/> stream from which the object is deserialized.</param>
        /// <param name="dictionary">The <see cref="Dictionary&lt;TKey, TValue&gt;"/> to be deserialized.</param>
        public static void ReadXml(XmlReader reader, IDictionary<TKey, TValue> dictionary)
        {
            reader.MustNotBeNull(nameof(reader));
            dictionary.MustNotBeNull(nameof(dictionary));

            ////XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer? valueSerializer = null;
            var useConvert = typeof(TValue) == typeof(object) || typeof(IConvertible).IsAssignableFrom(typeof(TValue));
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            reader.ReadStartElement(DICTIONARY);
            if (reader.NodeType == XmlNodeType.None)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement(ITEM);

                ////reader.ReadStartElement(KEY);
                ////TKey key = (TKey)keySerializer.Deserialize(reader);
                ////reader.ReadEndElement();

                var key = (TKey) Convert.ChangeType(reader.ReadElementString(KEY), typeof(TKey),
                    CultureInfo.InvariantCulture);
                if (key == null)
                    continue;
                var value = default(TValue);
                if (useConvert)
                {
                    value = (TValue) Convert.ChangeType(reader.ReadElementString(VALUE), typeof(TValue),
                        CultureInfo.InvariantCulture);
                }
                else
                {
                    valueSerializer ??= new XmlSerializer(typeof(TValue));

                    if (reader.IsEmptyElement)
                    {
                        reader.ReadStartElement(VALUE);
                    }
                    else
                    {
                        reader.ReadStartElement(VALUE);
                        value = (TValue) valueSerializer.Deserialize(reader)!;
                        reader.ReadEndElement();
                    }
                }

                // TODO: elements (non)-nullability suspect
                if (dictionary.ContainsKey(key))
                    dictionary[key] = value!;
                else
                    dictionary.Add(key, value!);

                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts a <see cref="IDictionary&lt;TKey, TValue&gt;"/> into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="dictionary">The <see cref="Dictionary&lt;TKey, TValue&gt;"/> to be serialized.</param>
        public static void WriteXml(XmlWriter writer, IDictionary<TKey, TValue> dictionary)
        {
            writer.MustNotBeNull(nameof(writer));
            dictionary.MustNotBeNull(nameof(dictionary));

            ////XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer? valueSerializer = null;
            var useConvert = typeof(TValue) == typeof(object) || typeof(IConvertible).IsAssignableFrom(typeof(TValue));

            writer.WriteStartElement(DICTIONARY);

            foreach (var item in dictionary)
            {
                if (item.Value == null)
                    continue;

                writer.WriteStartElement(ITEM);

                ////writer.WriteStartElement(KEY);
                ////keySerializer.Serialize(writer, item.Key);
                ////writer.WriteEndElement();

                writer.WriteElementString(KEY,
                    (string) Convert.ChangeType(item.Key, typeof(string), CultureInfo.InvariantCulture));

                if (useConvert)
                {
                    writer.WriteElementString(VALUE,
                        (string) Convert.ChangeType(item.Value, typeof(string), CultureInfo.InvariantCulture));
                }
                else
                {
                    valueSerializer ??= new XmlSerializer(typeof(TValue));

                    writer.WriteStartElement(VALUE);
                    valueSerializer.Serialize(writer, item.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        #endregion

        #region IXmlSerializable implementation

        /// <summary>This property is reserved, apply the <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the
        /// class instead.</summary>
        /// <returns>An <see cref="System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is
        /// produced by the <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and
        /// consumed by the <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.</returns>
        public XmlSchema? GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            ReadXml(reader, this);
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, this);
        }

        #endregion
    }
}