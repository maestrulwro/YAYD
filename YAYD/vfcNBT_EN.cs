// nothing to be done here
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace VFC
{
    /// <summary>
    /// Everything related to Named Binary Tag format (thanks Notch).
    /// </summary>
    namespace NBT
    {
        static class NBTTests
        {
            /// <summary>
            /// Generates and returns a <c>CompoundTag</c> containing all possible values of <c>ByteTag</c>.
            /// </summary>
            /// <param name="name">The name of the <c>CompoundTag</c> returned.</param>
            /// <returns>The <c>CompoundTag</c>.</returns>
            public static CompoundTag AllSByteValuesAsCompound(string name = "All SByte values as compound")
            {
                CompoundTag ret = new CompoundTag(name);
                for (sbyte s = sbyte.MinValue; s <= sbyte.MaxValue; s++)
                    {
                        byte b = (byte)s;
                        ret.Content.Add(new ByteTag("Byte equivalent of .NET is " + b.ToString(), s));
                        if (s == sbyte.MaxValue)
                            break;
                    }
                return ret;
            }
            /// <summary>
            /// Generates and returns a <c>ByteArrayTag</c> containing all possible values of <c>ByteTag</c>.
            /// </summary>
            /// <param name="name">The name of the <c>ByteArrayTag</c> returned.</param>
            /// <returns>The <c>ByteArrayTag</c>.</returns>
            public static ByteArrayTag AllSByteValuesAsArray(string name = "All SByte values as array")
            {
                List<sbyte> vs = new List<sbyte>();
                for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
                    vs.Add(i);
                vs.Add(sbyte.MaxValue);
                return new ByteArrayTag(name, vs.ToArray());
            }
            /// <summary>
            /// Generates and returns a <c>IntArrayTag</c> containing powers of 2.
            /// </summary>
            /// <param name="name">The name of the tag returned.</param>
            /// <returns>The tag.</returns>
            public static IntArrayTag ValueForPowerOfTwo(string name="Values for 2^x")
            {
                List<int> vs = new List<int>();
                int i = 0;
                while (true)
                {
                    double p = Math.Pow(2, i);
                    if (p > int.MaxValue) return new IntArrayTag(name, vs.ToArray());
                    vs.Add((int)(-p));
                    vs.Add((int)p);
                    i++;
                }
            }
            /// <summary>
            /// Generates and returns a <c>CompoundTag</c> with max values of everything possible.
            /// </summary>
            /// <param name="name">Name of the tag.</param>
            /// <returns>The tag.</returns>
            public static CompoundTag MaxValuesOfEverything(string name="Max values of everything")
            {
                CompoundTag ret = new CompoundTag(name);
                ret.Content.Add(new ByteTag("MaxSByte", sbyte.MaxValue));
                ret.Content.Add(new ShortTag("MaxShort", short.MaxValue));
                ret.Content.Add(new IntTag("MaxInt", int.MaxValue));
                ret.Content.Add(new LongTag("MaxLong", long.MaxValue));
                ret.Content.Add(new SingleTag("MaxSingle", Single.MaxValue));
                ret.Content.Add(new DoubleTag("MaxDouble", double.MaxValue));
                ret.Content.Add(new StringTag("MaxString", "String to maximum! (what really is string.MaxValue?)"));
                return ret;
            }
            /// <summary>
            /// Generates and returns a <c>CompoundTag</c> with min values of everything possible.
            /// </summary>
            /// <param name="name">Name of the tag.</param>
            /// <returns>The tag.</returns>
            public static CompoundTag MinValuesOfEverything(string name = "Min values of everything")
            {
                CompoundTag ret = new CompoundTag(name);
                ret.Content.Add(new ByteTag("MinSByte", sbyte.MinValue));
                ret.Content.Add(new ShortTag("MinShort", short.MinValue));
                ret.Content.Add(new IntTag("MinInt", int.MinValue));
                ret.Content.Add(new LongTag("MinLong", long.MinValue));
                ret.Content.Add(new SingleTag("MinSingle", Single.MinValue));
                ret.Content.Add(new DoubleTag("MinDouble", double.MinValue));
                ret.Content.Add(new StringTag("MaxString", "String to minimum! (what really is string.MinValue?)"));
                return ret;
            }
            /// <summary>
            /// Generates and returns a <c>CompoundTag</c> with max, min and other notable values of everything possible.
            /// </summary>
            /// <param name="name">Name of the tag.</param>
            /// <returns>The tag.</returns>
            public static CompoundTag NotableValuesOfEverything(string name = "Notable values of everything")
            {
                CompoundTag ret = new CompoundTag(name);
                ret.Content.Add(MaxValuesOfEverything());
                ret.Content.Add(MinValuesOfEverything());
                ret.Content.Add(new SingleTag("Single Epsilon", Single.Epsilon));
                ret.Content.Add(new DoubleTag("Double Epsilon", double.Epsilon));
                ret.Content.Add(new SingleTag("Single +∞", Single.PositiveInfinity));
                ret.Content.Add(new SingleTag("Single -∞", Single.NegativeInfinity));
                ret.Content.Add(new DoubleTag("Double +∞", double.PositiveInfinity));
                ret.Content.Add(new DoubleTag("Double -∞", double.NegativeInfinity));
                ret.Content.Add(new SingleTag("Single NaN", Single.NaN));
                ret.Content.Add(new DoubleTag("Double NaN", double.NaN));
                return ret;
            }
            /// <summary>
            /// Generates and returns a <c>StringTag</c> with a very long string inside.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="c">The char to repeat.</param>
            /// <param name="repeat">How many times to repeat the char.</param>
            /// <returns>The tag.</returns>
            public static StringTag VeryLongString(string name="Very long string", char c= '◼', int repeat=35000)
            {
                string s = "";
                for (int i = 0; i < repeat; i++)
                    s += c;
                return new StringTag(name, s);
            }
        }
        /// <summary>
        /// Contains functions related to reading and writing in Big Endian.
        /// </summary>
        static class ReadWriteFunctionsForBigEndian
        {
            /// <summary>
            /// Reads a string from a Big Endian stream.
            /// </summary>
            /// <param name="str">Big Endian stream to read from.</param>
            /// <returns>Read string.</returns>
            public static string StringRead(Stream str)
            {
                // utilization of Encoding.UTF8.GetString() inspired by https://github.com/fragmer/fNbt/blob/master/fNbt/NbtBinaryReader.cs
                // before it was reading only ASCII-compatible strings
                int char_no = str.ReadByte() * 256 + str.ReadByte();
                //int char_no = new ShortTag(str, false).Value;
                byte[] buf = new byte[char_no];
                str.Read(buf, 0, char_no);
                return Encoding.UTF8.GetString(buf);
            }
            /// <summary>
            /// Reads an integer from a Big Endian stream.
            /// </summary>
            /// <param name="str">The stream to read from.</param>
            /// <returns>The read integer.</returns>
            public static int IntRead(Stream str)
            {
                return BitConverter.ToInt32(ArrayRead(str, 4), 0);
            }
            /// <summary>
            /// Reads a given number of bytes from the stream and reverses it if it is necessary.
            /// </summary>
            /// <param name="str">The stream to read from.</param>
            /// <param name="nr_baiti">The number of bytes to read.</param>
            /// <returns>Array containing the read bytes.</returns>
            public static byte[] ArrayRead(Stream str, int bytes_no)
            {
                byte[] res = new byte[bytes_no];
                str.Read(res, 0, bytes_no);
                if (BitConverter.IsLittleEndian) Array.Reverse(res);
                return res;
            }
            /// <summary>
            /// Writes a given string into the given stream.
            /// </summary>
            /// <param name="srm">The stream to write to.</param>
            /// <param name="sea">The string to write.</param>
            public static void StringWrite(Stream srm, string sea)
            {
                // utilization of Encoding.UTF8.GetString() inspired by https://github.com/fragmer/fNbt/blob/master/fNbt/NbtBinaryReader.cs
                // before it was writing only ASCII-compatible strings
                byte[] buf = Encoding.UTF8.GetBytes(sea);
                if (buf.Length < 32767)
                {
                    srm.WriteByte((byte)(buf.Length/256));
                    srm.WriteByte((byte)(buf.Length%256));
                    srm.Write(buf, 0, buf.Length);
                }
                else
                {
                    srm.WriteByte(127);
                    srm.WriteByte(255);
                    srm.Write(buf, 0, 32767);
                }
            }
            /// <summary>
            /// Writes a given integer into the given stream.
            /// </summary>
            /// <param name="srm">The stream to write to.</param>
            /// <param name="sea">The integer to write.</param>
            public static void IntWrite(Stream str, int nr)
            {
                ArrayWrite(str, BitConverter.GetBytes((int)nr));
            }
            /// <summary>
            /// Writes a given byte array into the given stream.
            /// </summary>
            /// <param name="srm">The stream to write to.</param>
            /// <param name="sea">The array to write.</param>
            public static void ArrayWrite(Stream str, byte[] de_scris)
            {
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris);
                str.Write(de_scris, 0, de_scris.Length);
            }
        }
        /// <summary>
        /// Base tag.
        /// </summary>
        abstract class AbstrTag
        {
            /// <summary>
            /// The type of the tag.
            /// </summary>
            public abstract TagTypes TagType { get; }
            /// <summary>
            /// The payload of the tag, as object.
            /// </summary>
            /// <remarks>This is the <c>Value</c> or the <c>Content</c> property presented as an object.</remarks>
            public abstract object Payload { get; set; }
            /// <summary>
            /// The name of the tag.
            /// </summary>
            public string Name { get; set; } = "";
            /// <summary>
            /// All the available tag types.
            /// </summary>
            public enum TagTypes
            {
                TAG_End = 0,
                TAG_Byte = 1,
                TAG_Short = 2,
                TAG_Int = 3,
                TAG_Long = 4,
                TAG_Float = 5,
                TAG_Double = 6,
                TAG_Byte_Array = 7,
                TAG_String = 8,
                TAG_List = 9,
                TAG_Compound = 10,
                TAG_Int_Array = 11
            }
            /// <summary>
            /// To be overriden by the function that represents the tag in human readable format.
            /// </summary>
            /// <returns>String representing the tag, human readable format.</returns>
            /// <remarks>Will also contain data of children tags where applicable.</remarks>
            public override abstract string ToString();
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have; used especially for <c>CompoundTag</c> and <c>ListTag</c>.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags where applicable; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public abstract string ToString(int indent, int increment = 1);
            /// <summary>
            /// To be overriden by the function that writes in the <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to write to.</param>
            /// <param name="writeheader"><c>True</c> to also write the header (it is missing in a <c>ListTag</c>).</param>
            public abstract void WriteInStream(Stream str, bool writeheader = true);
            /// <summary>
            /// Returns the next tag read from the given <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <returns>The read tag.</returns>
            public static AbstrTag TagRead(Stream str)
            {
                byte b = (byte)str.ReadByte();
                switch (b)
                {
                    case 0:
                        return new EndTag(str);
                    case 1:
                        return new ByteTag(str);
                    case 2:
                        return new ShortTag(str);
                    case 3:
                        return new IntTag(str);
                    case 4:
                        return new LongTag(str);
                    case 5:
                        return new SingleTag(str);
                    case 6:
                        return new DoubleTag(str);
                    case 7:
                        return new ByteArrayTag(str);
                    case 8:
                        return new StringTag(str);
                    case 9:
                        return new ListTag(str);
                    case 10:
                        return new CompoundTag(str);
                    case 11:
                        return new IntArrayTag(str);
                    default:
                        throw new NotImplementedException("Code not implemented yet!" + b.ToString());
                }
            }
        }
        /// <summary>
        /// End tag used only in reading.
        /// I should delete it sometime...
        /// ID=0
        /// </summary>
        class EndTag : AbstrTag
        {
            /// <summary>
            /// Returns null. Because <c>EndTag</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return null;
                }

                set
                {

                }
            }
            /// <summary>
            /// Returns <c>TagTypes.TAG_End</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_End;
                }
            }
            /// <summary>
            /// Will throw a <c>NotImplementedException</c>!
            /// </summary>
            /// <param name="str"></param>
            /// <param name="writeheader"></param>
            /// <exception cref="NotImplementedException">Will be surely thrown!</exception>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Will throw an <c>NotImplementedException</c>!
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotImplementedException">Will be surely thrown!</exception>
            public override string ToString()
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Will throw an <c>NotImplementedException</c>!
            /// </summary>
            /// <param name="indent"></param>
            /// <param name="increment"></param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException">Will be surely thrown!</exception>
            public override string ToString(int indent, int increment = 1)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Nothing!
            /// </summary>
            /// <param name="str"></param>
            public EndTag(Stream str)
            {

            }
        }
        /// <summary>
        /// Tag containing a <c>List</c> of other (named) tags.
        /// ID=10
        /// </summary>
        class CompoundTag : AbstrTag
        {
            /// <summary>
            /// The <c>List</c> containing the elements as <c>AbstrTag</c>.
            /// </summary>
            public List<AbstrTag> Content { get; set; }
            /// <summary>
            /// Returns the first element with the given name, or throws an error.
            /// </summary>
            /// <param name="name">The name to search for.</param>
            /// <param name="throwerror"><c>True</c> to throw an error if no element is found; false to return <c>null</c>.</param>
            /// <returns>The element with the given name.</returns>
            /// <exception cref="KeyNotFoundException">Thrown if no element has the given name and <c>throwerror</c> is <c>True</c>.</exception>
            public AbstrTag FindElementByName(string name, bool throwerror = true)
            {
                foreach (AbstrTag elem in Content) if (elem.Name == name) return elem;
                if (throwerror)
                    throw new KeyNotFoundException();
                else
                    return null;
            }
            /// <summary>
            /// The <c>Content</c> as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Content;
                }

                set
                {
                    Content = (List<AbstrTag>)value;
                }
            }
            /// <summary>
            /// Returns <c>TagTypes.TAG_Compound</c>;
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Compound;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(10);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                foreach (AbstrTag elem in Content) elem.WriteInStream(str);
                str.WriteByte(0);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Hooman readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Compound>" + Name + " (" + Content.Count + " elemente)");
                foreach (AbstrTag elem in Content) sb.Append(Environment.NewLine + elem.ToString(indent + increment, increment));
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public CompoundTag(Stream str, bool readheader = true)
            {
                Content = new List<AbstrTag>();
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                AbstrTag ultimul_citit = null;
                do
                {
                    if (ultimul_citit != null) Content.Add(ultimul_citit);
                    ultimul_citit = AbstrTag.TagRead(str);

                } while (ultimul_citit.TagType != TagTypes.TAG_End);
            }
            /// <summary>
            /// Constructor for new empty instance.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            public CompoundTag(string name)
            {
                Name = name;
                Content = new List<AbstrTag>();
            }
        }
        /// <summary>
        /// Tag containing a <c>sbyte</c>.
        /// ID=1
        /// </summary>
        class ByteTag : AbstrTag
        {
            /// <summary>
            /// The <c>sbyte</c> contained.
            /// </summary>
            public sbyte Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (sbyte)value;
                }
            }
            /// <summary>
            /// Returns <c>TagTypes.TAG_Byte</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Byte;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(1);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                str.WriteByte((byte)(Value));
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Byte>" + Name + "=" + Value.ToString());
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public ByteTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                Value = (sbyte)(str.ReadByte());
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public ByteTag(string name, sbyte val)
            {
                Name = name;
                this.Value = val;
            }
        }
        /// <summary>
        /// Tag containing a string.
        /// ID=8
        /// </summary>
        class StringTag : AbstrTag
        {
            /// <summary>
            /// The <c>string</c> contained.
            /// </summary>
            public string Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (string)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_String</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_String;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(8);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                ReadWriteFunctionsForBigEndian.StringWrite(str, Value);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("String>" + Name + " (" + Value.Length + " caractere) =" + Value);
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public StringTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                Value = ReadWriteFunctionsForBigEndian.StringRead(str);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public StringTag(string name, string val)
            {
                Name = name;
                this.Value = val;
            }
        }
        /// <summary>
        /// Tag containing an <c>int16</c>.
        /// ID=2
        /// </summary>
        class ShortTag : AbstrTag
        {
            /// <summary>
            /// The <c>short</c> (<c>int16</c>) contained.
            /// </summary>
            public short Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (short)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Short</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Short;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(2);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris = BitConverter.GetBytes(Value);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris);
                str.Write(de_scris, 0, 2);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Short>").Append(Name).Append("=").Append(Value.ToString());
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public ShortTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] citit = new byte[2];
                str.Read(citit, 0, 2);
                if (BitConverter.IsLittleEndian) Array.Reverse(citit);
                Value = BitConverter.ToInt16(citit, 0);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public ShortTag(string name, short val)
            {
                Name = name;
                Value = val;
            }
        }
        /// <summary>
        /// Tag containing a <c>int32</c>.
        /// ID=3
        /// </summary>
        class IntTag : AbstrTag
        {
            /// <summary>
            /// The <c>int32</c> contained.
            /// </summary>
            public int Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (int)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Int</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Int;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(3);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris = BitConverter.GetBytes(Value);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris); //de la little endian la big endian
                str.Write(de_scris, 0, 4);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Int32>").Append(Name).Append("=").Append(Value);
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public IntTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] citit = new byte[4];
                str.Read(citit, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(citit); //de la little endian la big endian
                Value = BitConverter.ToInt32(citit, 0);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public IntTag(string name, int val)
            {
                Name = name;
                Value = val;
            }
        }
        /// /// <summary>
        /// Tag containing a int64.
        /// ID=4
        /// </summary>
        class LongTag : AbstrTag
        {
            /// <summary>
            /// The <c>long</c> contained.
            /// </summary>
            public long Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (long)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.Tag_Long</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Long;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(4);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris = BitConverter.GetBytes(Value);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris); //de la little endian la big endian
                str.Write(de_scris, 0, 8);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Int64>").Append(Name).Append("=").Append(Value);
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public LongTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] citit = new byte[8];
                str.Read(citit, 0, 8);
                if (BitConverter.IsLittleEndian) Array.Reverse(citit); //de la little endian la big endian
                Value = BitConverter.ToInt64(citit, 0);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public LongTag(string name, long val)
            {
                Name = name;
                Value = val;
            }
        }
        /// <summary>
        /// Tag containing a float/single (4 bytes/32 bits).
        /// ID=5
        /// </summary>
        class SingleTag : AbstrTag
        {
            /// <summary>
            /// The <c>float</c> contained.
            /// </summary>
            public float Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (float)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Float</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Float;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(5);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris = BitConverter.GetBytes(Value);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris); //de la little endian la big endian
                str.Write(de_scris, 0, 4);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Single>").Append(Name).Append("=").Append(Value);
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public SingleTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] citit = new byte[4];
                str.Read(citit, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(citit); //de la little endian la big endian
                Value = BitConverter.ToSingle(citit, 0);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public SingleTag(string name, float val)
            {
                Name = name;
                Value = val;
            }
        }
        /// <summary>
        /// Tag containing a double (8 bytes/64 bits).
        /// ID=6
        /// </summary>
        class DoubleTag : AbstrTag
        {
            /// <summary>
            /// The <c>double</c> contained.
            /// </summary>
            public double Value { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Value;
                }

                set
                {
                    Value = (double)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Double</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Double;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(6);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris = BitConverter.GetBytes(Value);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris); //de la little endian la big endian
                str.Write(de_scris, 0, 8);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("Double>").Append(Name).Append("=").Append(Value);
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public DoubleTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] citit = new byte[8];
                str.Read(citit, 0, 8);
                if (BitConverter.IsLittleEndian) Array.Reverse(citit); //de la little endian la big endian
                Value = BitConverter.ToDouble(citit, 0);
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public DoubleTag(string name, double val)
            {
                Name = name;
                Value = val;
            }
        }
        /// <summary>
        /// Tag containing a sbyte vector.
        /// ID=7
        /// </summary>
        class ByteArrayTag : AbstrTag
        {
            /// <summary>
            /// The <c>sbyte[]</c> contained.
            /// </summary>
            public sbyte[] Continut { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Continut;
                }

                set
                {
                    Continut = (sbyte[])value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Byte_Array</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Byte_Array;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(7);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris_lungime = BitConverter.GetBytes(Continut.Length);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris_lungime);
                str.Write(de_scris_lungime, 0, 4);
                foreach (sbyte elem in Continut) str.WriteByte((byte)elem);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("ArrayByte>").Append(Name).Append(" (").Append(Continut.Length).Append(" elemente)");
                foreach (sbyte elem in Continut)
                {
                    sb.Append(Environment.NewLine);
                    for (int i = 0; i < indent + increment; i++) sb.Append(" ");
                    sb.Append(elem);
                }
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public ByteArrayTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] de_transformat = new byte[4];
                str.Read(de_transformat, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_transformat);
                Continut = new sbyte[BitConverter.ToInt32(de_transformat, 0)];
                for (int i = 0; i < Continut.Length; i++) Continut[i] = (sbyte)str.ReadByte();
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public ByteArrayTag(string name, sbyte[] val)
            {
                Name = name;
                Continut = val;
            }
        }
        /// <summary>
        /// Tag containing an int32 array.
        /// ID=11
        /// </summary>
        class IntArrayTag : AbstrTag
        {
            /// <summary>
            /// The <c>int[]</c> contained.
            /// </summary>
            public int[] Continut { get; set; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Continut;
                }

                set
                {
                    Continut = (int[])value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_Int_Array</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_Int_Array;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(11);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                byte[] de_scris_lungime = BitConverter.GetBytes(Continut.Length);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_scris_lungime);
                str.Write(de_scris_lungime, 0, 4);
                byte[] de_scris_Value;
                for (int i = 0; i < Continut.Length; i++)
                {
                    de_scris_Value = BitConverter.GetBytes(Continut[i]);
                    if (BitConverter.IsLittleEndian) Array.Reverse(de_scris_Value);
                    str.Write(de_scris_Value, 0, 4);
                }
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("ArrayInt32>").Append(Name).Append(" (").Append(Continut.Length).Append(" elemente)");
                foreach (int elem in Continut)
                {
                    sb.Append(Environment.NewLine);
                    for (int i = 0; i < indent + increment; i++) sb.Append(" ");
                    sb.Append(elem);
                }
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public IntArrayTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                byte[] de_transformat = new byte[4];
                str.Read(de_transformat, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(de_transformat);
                Continut = new int[BitConverter.ToInt32(de_transformat, 0)];
                for (int i = 0; i < Continut.Length; i++)
                {
                    str.Read(de_transformat, 0, 4);
                    if (BitConverter.IsLittleEndian) Array.Reverse(de_transformat);
                    Continut[i] = BitConverter.ToInt32(de_transformat, 0);
                }
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="val">The value of the tag.</param>
            public IntArrayTag(string name, int[] val)
            {
                Name = name;
                Continut = val;
            }
        }
        /// <summary>
        /// Tag containing a list of tags of given type.
        /// ID=9
        /// </summary>
        class ListTag : AbstrTag
        {
            /// <summary>
            /// The <c>List</c> contained.
            /// </summary>
            public List<AbstrTag> Continut { get; set; }
            /// <summary>
            /// The tag type of the content.
            /// </summary>
            public TagTypes Tip_continut { get; }
            /// <summary>
            /// The tag type as byte.
            /// </summary>
            public byte Tip_continut_ca_byte { get; }
            /// <summary>
            /// The payload of the tag, as <c>object</c>.
            /// </summary>
            public override object Payload
            {
                get
                {
                    return Continut;
                }

                set
                {
                    Continut = (List<AbstrTag>)value;
                }
            }
            /// <summary>
            /// Returns <c>TagType.TAG_List</c>.
            /// </summary>
            public override TagTypes TagType
            {
                get
                {
                    return TagTypes.TAG_List;
                }
            }
            /// <summary>
            /// Writes this tag in a <c>Stream</c>.
            /// </summary>
            /// <param name="str">The Stream to write to.</param>
            /// <param name="writeheader"><c>True</c> to write the ID and the name.</param>
            public override void WriteInStream(Stream str, bool writeheader = true)
            {
                if (writeheader)
                {
                    str.WriteByte(9);
                    ReadWriteFunctionsForBigEndian.StringWrite(str, Name);
                }
                str.WriteByte(Tip_continut_ca_byte);
                ReadWriteFunctionsForBigEndian.IntWrite(str, Continut.Count);
                foreach (AbstrTag elem in Continut) elem.WriteInStream(str, false);
            }
            /// <summary>
            /// Represents this tag in human readable format.
            /// </summary>
            /// <returns>Human readable format of this tag and its children.</returns>
            public override string ToString()
            {
                return this.ToString(0);
            }
            /// <summary>
            /// <c>ToString()</c> but having indentation options.
            /// </summary>
            /// <param name="indent">Number of spaces preceding the string representation of this element.</param>
            /// <param name="increment">Number of spaces added to the indent, that children of this element will have.</param>
            /// <returns>String representing the tag, human readable format. Preceded by <c>indent</c> number of spaces.</returns>
            /// <remarks>Will also contain data of children tags; those will be preceded by <c>indent + increment</c> number of spaces.</remarks>
            public override string ToString(int indent, int increment = 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < indent; i++) sb.Append(" ");
                sb.Append("List>" + Name + " (" + Continut.Count + " elemente)");
                foreach (AbstrTag elem in Continut) sb.Append(Environment.NewLine + elem.ToString(indent + increment, increment));
                return sb.ToString();
            }
            /// <summary>
            /// Constructor reading from <c>Stream</c>.
            /// </summary>
            /// <param name="str">The <c>Stream</c> to read from.</param>
            /// <param name="readheader">Wether or not to read the <c>Name</c>, applicable for <c>ListTag</c>.</param>
            public ListTag(Stream str, bool readheader = true)
            {
                if (readheader) Name = ReadWriteFunctionsForBigEndian.StringRead(str);
                Tip_continut_ca_byte = (byte)str.ReadByte();
                Continut = new List<AbstrTag>();
                int nr_elemente_de_citit = ReadWriteFunctionsForBigEndian.IntRead(str);
                switch (Tip_continut_ca_byte)
                {
                    case 1:
                        Tip_continut = TagTypes.TAG_Byte;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new ByteTag(str, false));
                        break;
                    case 2:
                        Tip_continut = TagTypes.TAG_Short;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new ShortTag(str, false));
                        break;
                    case 3:
                        Tip_continut = TagTypes.TAG_Int;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new IntTag(str, false));
                        break;
                    case 4:
                        Tip_continut = TagTypes.TAG_Long;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new LongTag(str, false));
                        break;
                    case 5:
                        Tip_continut = TagTypes.TAG_Float;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new SingleTag(str, false));
                        break;
                    case 6:
                        Tip_continut = TagTypes.TAG_Double;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new DoubleTag(str, false));
                        break;
                    case 7:
                        Tip_continut = TagTypes.TAG_Byte_Array;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new ByteArrayTag(str, false));
                        break;
                    case 8:
                        Tip_continut = TagTypes.TAG_String;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new StringTag(str, false));
                        break;
                    case 9:
                        Tip_continut = TagTypes.TAG_List;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new ListTag(str, false));
                        break;
                    case 10:
                        Tip_continut = TagTypes.TAG_Compound;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new CompoundTag(str, false));
                        break;
                    case 11:
                        Tip_continut = TagTypes.TAG_Int_Array;
                        for (int i = 0; i < nr_elemente_de_citit; i++) Continut.Add(new IntArrayTag(str, false));
                        break;
                    default:
                        throw new NotImplementedException("Cod neimplementat:" + Tip_continut_ca_byte);
                }
            }
            /// <summary>
            /// Constructor for new instance with given value.
            /// </summary>
            /// <param name="name">The name of the tag.</param>
            /// <param name="tip_val">The type of the tags contained.</param>
            /// <param name="val">The value of the tag.</param>
            public ListTag(string name, TagTypes tip_val, AbstrTag[] val)
            {
                Name = name;
                Tip_continut = tip_val;
                Tip_continut_ca_byte = (byte)Tip_continut;
                Continut = new List<AbstrTag>();
                foreach (AbstrTag elem in val) Continut.Add(elem);
            }
        }
        /// <summary>
        /// Represents a NBT file on disk.
        /// </summary>
        class NBTFile
        {
            public enum CompressionMethods
            {
                Uncompressed,
                Gzip,
                Zlib,
                Auto
            }
            /// <summary>
            /// The compression method of the file.
            /// </summary>
            public CompressionMethods CompressionMethod { get; set; }
            /// <summary>
            /// The location of file, as string.
            /// </summary>
            public string LocationOfFile { get; set; }
            /// <summary>
            /// The content of the file.
            /// </summary>
            public AbstrTag Content { get; set; }
            /// <summary>
            /// New instance from file.
            /// </summary>
            /// <param name="loc">Location of the file.</param>
            /// <param name="method">Method of reading the file.</param>
            public NBTFile(string loc, CompressionMethods method= CompressionMethods.Auto)
            {
                ReadFile(loc, method);
            }
            /// <summary>
            /// New instance from a given tag.
            /// </summary>
            /// <param name="cont">The tag as root of file.</param>
            /// <param name="loc">The location of file (may not exist yet).</param>
            /// <param name="method">The method in which to read/write the file.</param>
            public NBTFile(AbstrTag cont, string loc, CompressionMethods method= CompressionMethods.Uncompressed)
            {
                if (cont == null)
                    Content = new CompoundTag("unnamed root (of all evil)");
                else Content = cont;
                LocationOfFile = loc;
                CompressionMethod = method;
            }
            /// <summary>
            /// Writes the file.
            /// </summary>
            public void WriteFile()
            {
                WriteFile(LocationOfFile, CompressionMethod);
            }
            private void ReadFile(string loc, CompressionMethods method)
            {
                switch (method)
                {
                    case CompressionMethods.Uncompressed:
                        using (System.IO.Stream s = File.Open(loc, FileMode.Open))
                        {
                            Content = AbstrTag.TagRead(s);
                            return;
                        }
                    case CompressionMethods.Gzip:
                        using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(File.Open(loc, FileMode.Open), System.IO.Compression.CompressionMode.Decompress))
                        {
                            Content = AbstrTag.TagRead(gs);
                            return;
                        }
                    case CompressionMethods.Zlib:
                        using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(File.Open(loc, FileMode.Open), System.IO.Compression.CompressionMode.Decompress))
                        {
                            Content = AbstrTag.TagRead(ds);
                            return;
                        }
                    case CompressionMethods.Auto:
                        try
                        {
                            ReadFile(loc, CompressionMethods.Zlib);
                            return;
                        }
                        catch { }
                        try
                        {
                            ReadFile(loc, CompressionMethods.Gzip);
                            return;
                        }
                        catch { }
                        ReadFile(loc, CompressionMethods.Uncompressed);
                        return;
                    default:
                        break;
                }
            }
            private void WriteFile(string loc, CompressionMethods method)
            {
                switch (method)
                {
                    case CompressionMethods.Gzip:
                        using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(File.Open(loc, FileMode.Create), System.IO.Compression.CompressionMode.Compress))
                        {
                            Content.WriteInStream(gs);
                            return;
                        }
                    case CompressionMethods.Zlib:
                        using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(File.Open(loc, FileMode.Create), System.IO.Compression.CompressionMode.Compress))
                        {
                            Content.WriteInStream(ds);
                            return;
                        }
                    default:
                        using (System.IO.Stream s = File.Open(loc, FileMode.Create))
                        {
                            Content.WriteInStream(s);
                            return;
                        }
                }
            }
        }
    }
}

