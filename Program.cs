using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;

using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;

namespace MCS6_Tidier
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!System.IO.File.Exists(args.Last()))
            {
                throw new System.Exception("Missing file: " + args.Last());
            }

            DdiValidator validator = new DdiValidator(args.Last(), DdiFileFormat.Ddi32);
            XDocument doc;
            if (validator.Validate())
            {
                doc = validator.ValidatedXDocument;
            }
            else
            {
                throw new System.Exception("Invalid file: " + args.Last());
            }

            Ddi32Deserializer deserializer = new Ddi32Deserializer();
            HarmonizationResult harmonized = deserializer.HarmonizeIdentifiers(doc, DdiFileFormat.Ddi32);

            DdiInstance instance = deserializer.GetDdiInstance(doc.Root);
            var rp = instance.ResourcePackages.First();

            var new_instance = new DdiInstance()
            { 
                CompositeId = instance.CompositeId,
                DublinCoreMetadata = instance.DublinCoreMetadata
            };

            var new_rp = new ResourcePackage()
            {
                CompositeId = rp.CompositeId,
                DublinCoreMetadata = rp.DublinCoreMetadata
            };
            new_instance.ResourcePackages.Add(new_rp);

            var new_ccs = new ControlConstructScheme()
            {
                CompositeId = rp.ControlConstructSchemes.First().CompositeId
            };
            new_ccs.Label.Copy(rp.ControlConstructSchemes.First().Label);
            new_rp.ControlConstructSchemes.Add(new_ccs);
            foreach (var ccs in rp.ControlConstructSchemes)
            {
                foreach (var child in ccs.GetChildren())
                {
                    new_ccs.AddChild(child);
                }
            }

            var new_qs = new QuestionScheme()
            {
                CompositeId = rp.QuestionSchemes.First().CompositeId
            };
            new_qs.Label.Copy(rp.QuestionSchemes.First().Label);
            new_rp.QuestionSchemes.Add(new_qs);
            foreach (var qs in rp.QuestionSchemes)
            {
                foreach (var child in qs.GetChildren())
                {
                    new_qs.AddChild(child);
                }
            }

            var new_cs = new CategoryScheme()
            {
                CompositeId = rp.CategorySchemes.First().CompositeId
            };
            new_cs.Label.Copy(rp.CategorySchemes.First().Label);
            new_rp.CategorySchemes.Add(new_cs);
            foreach (var cs in rp.CategorySchemes)
            {
                foreach (var child in cs.GetChildren())
                {
                    new_cs.AddChild(child);
                }
            }

            var new_cls = new CodeListScheme()
            {
                CompositeId = rp.CodeListSchemes.First().CompositeId
            };
            new_cls.Label.Copy(rp.CodeListSchemes.First().Label);
            new_rp.CodeListSchemes.Add(new_cls);
            foreach (var cls in rp.CodeListSchemes)
            {
                foreach (var child in cls.GetChildren())
                {
                    new_cls.AddChild(child);
                }
            }

            var new_is = new InstrumentScheme()
            {
                CompositeId = rp.InstrumentSchemes.First().CompositeId
            };
            new_is.Label.Copy(rp.InstrumentSchemes.First().Label);
            new_rp.InstrumentSchemes.Add(new_is);
            foreach (var old_is in rp.InstrumentSchemes)
            {
                foreach (var child in old_is.GetChildren())
                {
                    new_is.AddChild(child);
                }
            }

            var serializer = new Ddi32Serializer { UseConciseBoundedDescription = false };
            var output = serializer.Serialize(new_instance);
            output.Save(@"output.xml");
        }
    }
}
