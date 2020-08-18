using System;
using System.IO;

/// <summary>
/// Class representing a binary file capable of reading or writing.
/// </summary>
public class BinaryFile : IDisposable
{
	#region Fields

	public enum BinaryMode { READ, WRITE };

	private FileStream filestream;
	private BinaryWriter writer;
	private BinaryReader reader;
	private BinaryMode mode;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="BinaryFile"/> class.
	/// </summary>
	/// <param name="filepath">The fle path of the binary file</param>
	/// <param name="mode">The read or write mode of the binary file</param>
	public BinaryFile(string filepath, BinaryMode mode = BinaryMode.READ) 
	{
		filepath = FileSystem.EnsureFilePath(filepath);
		this.mode = mode;
		if (mode.Equals(BinaryMode.WRITE)) 
		{
			filestream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
			writer = new BinaryWriter(filestream);
		} 
		else 
		{
			filestream = System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read);
			reader = new BinaryReader(filestream);
		}
	}

	#endregion Constructors

	#region Public Properties

	/// <summary>
	/// Gets the stream length of the binary file
	/// </summary>
	public long? StreamLength => writer?.BaseStream?.Length ?? reader?.BaseStream?.Length;

	#endregion Public Properties

	#region Public Methods

	/// <summary>
	/// Writes a float value to the binary file.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(float value) {
		writer.Write(value);
	}

	 /// <summary>
	/// Writes a byte array to the binary file.
	/// </summary>
	/// <param name="value">The bytes to write</param>
	public void Write(byte[] bytes) {
		writer.Write(bytes);
	}

	/// <summary>
	/// Reads a float from the binary file.
	/// </summary>
	/// <returns>The float value read</returns>
	public float ReadSingle() 
	{
		return reader.ReadSingle();
	}

	/// <summary>
	/// Reads an array of bytes from the binary file.
	/// </summary>
	/// <param name="count">The length of bytes to read</param>
	/// <returns>The byte array read</returns>
	public byte[] Read(int count) 
	{
		return reader.ReadBytes(count);
	}

	/// <summary>
	/// Closes the binary file.
	/// </summary>
	public void Close() 
	{
		if (mode.Equals(BinaryMode.READ)) 
		{
			reader.Close();
		} 
		else 
		{
			writer.Close();
		}
	}

	/// <summary>
	/// Disposes of the held memory of the binary file.
	/// </summary>
	public void Dispose() 
	{
		filestream.Close();
	}
	
	#endregion Public Methods
}
