using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LakatosCardReader.Utils
{
    public class BER
    {
        public uint Tag { get; set; }
        public bool Primitive { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public List<BER> Children { get; set; } = new(); //List<BER>();


        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            // Pišemo Tag
            writer.Write(Tag);

            // Pišemo IsPrimitive kao 1 bajt (true = 1, false = 0)
            writer.Write(Primitive);

            if (Primitive)
            {
                // Ako je primitivni čvor, pišemo veličinu i podatke
                writer.Write(Data.Length);
                writer.Write(Data);
            }
            else
            {
                // Ako ima decu, pišemo broj dece i rekurzivno pišemo svako dete
                writer.Write(Children.Count);
                foreach (var child in Children)
                {
                    writer.Write(child.ToByteArray());
                }
            }

            return memoryStream.ToArray();
        }

        // Parsira BER podatke
        public static BER ParseBER(byte[] data)
        {
            var tree = new BER();

            // Parsiranje primitivnih i konstruktivnih tagova
            var (primitiveTags, constructedTags, err) = ParseBERLayer(data);

            if (err != null)
            {
                throw new InvalidOperationException($"Error while parsing BER layer: {err}");
            }
            // Provera da li je primitiveTags null
            if (primitiveTags == null)
            {
                throw new InvalidOperationException("Primitive tags cannot be null.");
            }
            // Dodavanje primitivnih tagova
            foreach (var tag in primitiveTags)
            {
                var node = new BER
                {
                    Tag = tag.Key,
                    Primitive = true,
                    Data = tag.Value
                };

                try
                {
                    tree.Add(node); // Dodavanje primitivnog čvora
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error adding primitive node with tag {tag.Key}: {ex.Message}", ex);
                }
            }
            // Provera da li je primitiveTags null
            if (constructedTags == null)
            {
                throw new InvalidOperationException("Constructed tags cannot be null.");
            }
            // Dodavanje konstruktivnih tagova
            foreach (var tag in constructedTags)
            {
                var node = new BER
                {
                    Tag = tag.Key,
                    Primitive = false,
                    Data = Array.Empty<byte>()//null // Konstruktivni čvorovi nemaju direktne podatke
                };

                try
                {
                    var subTree = ParseBER(tag.Value); // Rekurzivno parsiranje konstruktivnih čvorova
                    node.Children.AddRange(subTree.Children);

                    tree.Add(node); // Dodavanje konstruktivnog čvora
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error adding constructed node with tag {tag.Key}: {ex.Message}", ex);
                }
            }

            return tree;
        }


        // Parsira jedan nivo BER podataka i vraća primitivne i konstruktivne tagove
        private static (Dictionary<uint, byte[]>?, Dictionary<uint, byte[]>?, string?) ParseBERLayer(byte[] data)
        {
            var primitiveTags = new Dictionary<uint, byte[]>();
            var constructedTags = new Dictionary<uint, byte[]>();
            int offset = 0;

            while (true)
            {
                try
                {
                    // Pozivamo novu verziju ParseTag i dobijamo tuple
                    var (tag, primitive, tagOffsetDelta, tagError) = ParseTag(data, offset);

                    if (tagError != null)
                    {
                        return (null, null, tagError); // Vraćamo grešku ako postoji
                    }

                    offset += tagOffsetDelta; // Ažuriramo offset na osnovu povratne vrednosti

                    // Pozivamo ParseLength i dobijamo dužinu
                    var (length, lengthOffsetDelta, lengthError) = ParseLength(data, offset);

                    if (lengthError != null)
                    {
                        return (null, null, lengthError); // Vraćamo grešku ako postoji
                    }

                    offset += (int)lengthOffsetDelta;

                    if (offset + length > data.Length)
                    {
                        return (null, null, "Invalid length");
                    }

                    // Ekstrahujemo vrednost
                    byte[] value = data.Skip(offset).Take((int)length).ToArray();
                    offset += (int)length;

                    // Dodajemo u odgovarajući rečnik
                    if (primitive)
                    {
                        primitiveTags[tag] = value;
                    }
                    else
                    {
                        constructedTags[tag] = value;
                    }

                    if (offset == data.Length)
                    {
                        break; // Kraj podataka
                    }
                }
                catch (Exception ex)
                {
                    return (null, null, ex.Message);
                }
            }

            return (primitiveTags, constructedTags, null);
        }



        // Provera da li je tag primitivni
        private static bool IsPrimitive(uint tag)
        {
            return tag < 0x80;  // Primer: tag koji je manji od 0x80 je primitivni
        }

        // Parsira tag (deo kodiranja sa ID-om)
        //private static uint ParseTag(byte[] data, ref int offset)
        //{
        //    uint tag = data[offset];
        //    offset++;

        //    // Prošireni tagovi (ako su tagovi duži od 1 bajta)
        //    if ((tag & 0x1F) == 0x1F)
        //    {
        //        tag = (tag << 8) | data[offset];
        //        offset++;
        //    }

        //    return tag;
        //}
        private static (uint tag, bool primitive, int offset, string? error) ParseTag(byte[] data, int offset)
        {
            if (data.Length - offset <= 0)
            {
                return (0, false, offset, "Invalid length");
            }

            bool primitive = (data[offset] & 0b100000) == 0;

            uint tag;
            int originalOffset = offset;

            if ((data[offset] & 0x1F) != 0x1F)
            {
                tag = data[offset];
                offset++;
            }
            else if (data.Length - offset >= 2 && (data[offset + 1] & 0x80) == 0x00)
            {
                tag = (uint)((data[offset] << 8) | data[offset + 1]);
                offset += 2;
            }
            else if (data.Length - offset >= 3)
            {
                tag = (uint)(data[offset] << 16 | data[offset + 1] << 8 | data[offset + 2]);
                offset += 3;
            }
            else
            {
                return (0, false, originalOffset, "Invalid length");
            }

            return (tag, primitive, offset - originalOffset, null);
        }

        // Parsira dužinu (prvi byte sadrži dužinu)
        //private static uint ParseLength(byte[] data, ref int offset)
        //{
        //    uint length = data[offset];
        //    offset++;

        //    if (length >= 0x80)
        //    {
        //        int lengthBytes = (int)(length & 0x7F);
        //        length = 0;
        //        for (int i = 0; i < lengthBytes; i++)
        //        {
        //            length = (length << 8) | data[offset];
        //            offset++;
        //        }
        //    }

        //    return length;
        //}

        private static (uint length, uint offsetDelta, string? error) ParseLength(byte[] data, int offset)
        {
            if (data.Length - offset <= 0)
            {
                return (0, 0, "Invalid length");
            }

            uint firstByte = data[offset];
            uint length = 0;
            uint offsetDelta = 0;

            if (firstByte < 0x80)
            {
                length = firstByte;
                offsetDelta = 1;
            }
            else if (firstByte == 0x80)
            {
                return (0, 0, "Invalid format");
            }
            else if (firstByte == 0x81 && data.Length - offset >= 2)
            {
                length = data[offset + 1];
                offsetDelta = 2;
            }
            else if (firstByte == 0x82 && data.Length - offset >= 3)
            {
                length = (uint)((data[offset + 1] << 8) | data[offset + 2]);
                offsetDelta = 3;
            }
            else if (firstByte == 0x83 && data.Length - offset >= 4)
            {
                length = (uint)((data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
                offsetDelta = 4;
            }
            else if (firstByte == 0x84 && data.Length - offset >= 5)
            {
                length = (uint)((data[offset + 1] << 24) | (data[offset + 2] << 16) | (data[offset + 3] << 8) | data[offset + 4]);
                offsetDelta = 5;
            }
            else
            {
                return (0, 0, "Invalid length");
            }

            return (length, offsetDelta, null);
        }


        public byte[] Access(params uint[] address)
        {
           

            if (address.Length == 0)
            {
                
                return Data;
            }

           

            foreach (var child in Children)
            {
                

                if (child.Tag == address[0])
                {
                   
                    return child.Access(address[1..]); // Rekurzivni poziv sa ostatkom adrese
                }
            }

           
            throw new Exception($"Tag not found for address: {string.Join(", ", address)}");
        }


        public void AssignFrom(ref string target, params uint[] address)
        {
            try
            {
                var bytes = Access(address);
                target = System.Text.Encoding.UTF8.GetString(bytes); // Convert bytes to string
            }
            catch (Exception)
            {
                target = string.Empty; // Handle error by setting target to empty string
            }
        }


        public bool TryAssignFrom(ref string target, params uint[] address)
        {
            byte[] bytes = Access(address);
            if (bytes != null)
            {
                target = Encoding.UTF8.GetString(bytes);
                return true;
            }
            return false;
        }
        // Metoda za pristup podacima prema tagovima
        //public byte[] Access(params uint[] address)
        //{
        //    if (address.Length == 0)
        //    {
        //        return Data;
        //    }

        //    foreach (var child in Children)
        //    {
        //        if (child.Tag == address[0])
        //        {
        //            return child.Access(address[1..]);
        //        }
        //    }

        //    // Ako tag nije pronađen, vrati null umesto da baciš izuzetak
        //    return null;
        //}


        // Dodaje novi čvor u trenutni BER
        //public void Add(BER newNode)
        //{
        //    // Dodavanje novog čvora
        //    Children.Add(newNode);
        //}
        public void Add(BER newNode)
        {
            if (this.Primitive)
            {
                throw new InvalidOperationException("Can't add a value into primitive value");
            }

            // Pronađi čvor sa istim tagom
            var targetField = this.Children.FirstOrDefault(child => child.Tag == newNode.Tag);

            if (targetField == null)
            {
                // Ako ne postoji, dodaj novi čvor
                this.Children.Add(newNode);
            }
            else
            {
                if (targetField.Primitive == newNode.Primitive)
                {
                    if (targetField.Primitive)
                    {
                        // Ako su oba primitivna, zameni postojeći čvor novim
                        int index = this.Children.IndexOf(targetField);
                        if (index != -1)
                        {
                            this.Children[index] = newNode;
                        }
                    }
                    else
                    {
                        // Ako nisu primitivna, rekurzivno dodaj decu
                        foreach (var child in newNode.Children)
                        {
                            targetField.Add(child);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Types don't match");
                }
            }
        }
        // Pomoćna metoda za štampanje
        public string PrintTree()
        {
            return $"Tag: {Tag}, Data: {BitConverter.ToString(Data)}";
        }

        public void Merge(BER other)
        {
            if (this.Tag != other.Tag)
            {
                throw new InvalidOperationException("Tags don't match");
            }

            foreach (var child in other.Children)
            {
                this.Add(child);
            }
        }
    }
}
