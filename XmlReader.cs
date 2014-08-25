using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;

namespace WebRequestor {
   public class AttributeCollection {
      public String NodeName { get; set; }
      public Dictionary<string, string> Attributes { get; set;}

      public AttributeCollection(string nodeName) {
         this.NodeName = nodeName;
         Attributes = new Dictionary<string, string>();
      }
   }
   public class XmlReaderLocal {
      XmlTextReader reader;
      String xmlFil;
      public XmlReaderLocal(String xmlFil) {
         this.xmlFil = xmlFil;
      }
      /// <summary>
      ///  Read all attributes occurrences of one named node taken as input
      /// </summary>
      public AttributeCollection ReadFirstNodeAttributes(String nodename) {
         reader = new XmlTextReader(xmlFil);
         AttributeCollection attrCollection = new AttributeCollection(reader.Name);
         while (reader.Read()) {            
            switch (reader.NodeType) {
               case XmlNodeType.Element:
                  if (reader.Name.ToString() == nodename) {
                     if (reader.HasAttributes) {
                        //Console.WriteLine("Attributes of <" + reader.Name + ">");                                                
                        while (reader.MoveToNextAttribute()) {
                           attrCollection.Attributes.Add(reader.Name, reader.Value);
                        }
                        reader.MoveToElement();                        
                        reader.Close();                        
                     }                     
                  }
                  break;
               case XmlNodeType.Text:
                  break;
               case XmlNodeType.EndElement:
                  break;
            }
         }
         reader.Dispose();
         return attrCollection.Attributes.Count > 0 ? attrCollection : null;
      }
      /// <summary>
      ///  Read first occurrence of one named node taken as input
      /// </summary>
      public String ReadFirstNode(String nodename) {
         Boolean readValue = false;
         reader = new XmlTextReader(xmlFil);
         while (reader.Read()) {
            switch (reader.NodeType) {
               case XmlNodeType.Element:
                  if (reader.Name.ToString() == nodename) readValue = true;
                  break;
               case XmlNodeType.Text:
                  if (readValue == true) {
                     reader.Close();
                     return reader.Value.ToString();
                  }
                  break;
               case XmlNodeType.EndElement:
                  break;
            }
         }         
         return null;
      }
      /// <summary>
      ///  Read first occurrence of several named nodes taken as input
      /// </summary>
      public Hashtable ReadFirstOfNodes(String[] nodenames) {
         Hashtable resultingNodeValues = new Hashtable();
         Boolean readValue = false;
         reader = new XmlTextReader(xmlFil);
         while (reader.Read()) {
            foreach (String node in nodenames) {
               switch (reader.NodeType) {
                  case XmlNodeType.Element:
                     if (reader.Name.ToString() == node) readValue = true;
                     break;
                  case XmlNodeType.Text:
                     if (readValue == true && !resultingNodeValues.Contains(node)) {
                        resultingNodeValues.Add(node, reader.Value.ToString());
                        readValue = false;
                     }
                     break;
                  case XmlNodeType.EndElement:
                     break;
               }
            }
         }
         reader.Close();
         reader.Dispose();
         return resultingNodeValues;
      }
      /// <summary>
      ///  Read all servers disdinguished by the SERVER nodes
      /// </summary>
      public List<Hashtable> ReadAllServers() {
         Hashtable hashtable = new Hashtable();
         List<Hashtable> hashtableList = new List<Hashtable>();
         String currentNode = "";
         Boolean serverStart = false;
         reader = new XmlTextReader(xmlFil);
         while (reader.Read()) {
            switch (reader.NodeType) {
               case XmlNodeType.Element:
                  if (reader.Name.ToString() == "SERVER") serverStart = true;
                  if (serverStart == true) currentNode = reader.Name.ToString();
                  break;
               case XmlNodeType.Text:
                  if (serverStart == true) hashtable.Add(currentNode, reader.Value.ToString());
                  break;
               case XmlNodeType.EndElement:
                  if (reader.Name.ToString() == "SERVER") {
                     serverStart = false;
                     hashtableList.Add(new Hashtable(hashtable));
                     hashtable.Clear();
                  }
                  break;
            }
         }
         reader.Close();
         reader.Dispose();
         return hashtableList;
      }
   }
}
