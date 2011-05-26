using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

public class SyntaxTable
{
    protected String Name = "";
    protected TextBuffer Buffer = null;
    protected Dictionary<String, TextTag> KeyToTag = new Dictionary<String, TextTag>();
    protected Dictionary<String, TextTag> TagNameToTag = new Dictionary<String, TextTag>();
    protected int LastPosition = 0;

    protected SyntaxTable() { }

    protected void AddTag(String value)
    {
        TextTag tag = new TextTag(value);

        if (value.Contains(","))
        {
            String[] split = value.Split(',');
            Byte r = Byte.Parse(split[0]);
            Byte g = Byte.Parse(split[1]);
            Byte b = Byte.Parse(split[2]);

            tag.ForegroundGdk = new Gdk.Color(r, g, b);
        }

        else
            tag.Foreground = value;

        TagNameToTag[value] = tag;
    }

    public static SyntaxTable LoadFromFile(String filename)
    {
        SyntaxTable table = new SyntaxTable();

        try
        {
            FileStream fs = new FileStream(filename, FileMode.Open,
                                    FileAccess.Read, FileShare.Read);
            StreamReader reader = new StreamReader(fs);

            String line = reader.ReadLine();

            // Read in lines until empty
            do
            {
                if (!String.IsNullOrEmpty(line))
                {
                    // Get line to form of a=b
                    line = line.Replace(" ", "");
                    line = line.Replace("\t", "");
                    String[] split = line.Split('=');

                    if (split.Length != 2)
                        return null;

                    // Extract the property and value
                    String property = split[0];
                    String value = split[1];
                    String lowerprop = property.ToLower();
                    String lowervalue = value.ToLower();

                    if (lowerprop == "name")
                        table.Name = value;

                    else
                    {
                        if (!table.TagNameToTag.ContainsKey(lowervalue))
                            table.AddTag(lowervalue);
                        table.KeyToTag[property] = table.TagNameToTag[lowervalue];
                    }
                }

                line = reader.ReadLine();
            }
            while (!reader.EndOfStream || !String.IsNullOrEmpty(line));

            reader.Close();
        }

        catch (Exception)
        {
            return null;
        }

        return table;
    }

    public void SetBuffer(TextBuffer buffer)
    {
        // Remove tags from old buffer
        if (buffer != null)
            foreach (TextTag tag in TagNameToTag.Values)
                buffer.TagTable.Remove(tag);

        Buffer = buffer;

        // Then add to the new buffer
        foreach (TextTag tag in TagNameToTag.Values)
            buffer.TagTable.Add(tag);
    }

    public void Update(int last)
    {
        foreach (String word in KeyToTag.Keys)
        {
            TextTag tag = KeyToTag[word];
            int position = Buffer.Text.IndexOf(word, 0);
            int offset = word.Length;

            while (position != -1)
            {
                // Apply the tag to the range
                var start = Buffer.GetIterAtOffset(position);
                var end = Buffer.GetIterAtOffset(position + offset);
                Buffer.ApplyTag(tag, start, end);

                // Get the next position of this word
                position = Buffer.Text.IndexOf(word, position + offset);
            }
        }
    }
}

public partial class MainWindow : Gtk.Window
{
    protected SyntaxTable table;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

        table = SyntaxTable.LoadFromFile("highlight\\csharp.hl");

        if (table == null)
        {
            var dialog = new MessageDialog(this, DialogFlags.Modal,
                MessageType.Error, ButtonsType.Close, "Could not load Syntax tables");
            dialog.Title = "Error";

            if ((ResponseType) dialog.Run() == ResponseType.Close)
                Environment.Exit(0);
        }

        table.SetBuffer(logTextView.Buffer);
    }

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}
	
	protected virtual void OnClose(object sender, System.EventArgs e)
	{
		 // Reset the logTreeView and change the window back to original size
		 int width, height;
		 this.GetDefaultSize(out width, out height);
		 this.Resize(width, height);
		 
		 logTextView.Buffer.Text = "";
			
		 // Change the MainWindow Title back to the default.
		 this.Title = "Lets Read Crap";
	}
	
	protected virtual void OnExitActionActivated(object sender, System.EventArgs e)
	{
		Application.Quit();
	}
	
	protected virtual void OnOpen(object sender, System.EventArgs e)
	{
		// Reset the logTreeView and change the window back to original size
		int width, height;
		this.GetDefaultSize(out width, out height);
		this.Resize(width, height);
		  
		logTextView.Buffer.Text = "";

		// Create and display a fileChooserDialog
		FileChooserDialog chooser = new FileChooserDialog(
			"Please select a file to view ...",
			this,
			FileChooserAction.Open,
			"Cancel", ResponseType.Cancel,
			"Open", ResponseType.Accept);

		if (chooser.Run() == (int)ResponseType.Accept)
		{
			// Open the file for reading.
			System.IO.StreamReader file = System.IO.File.OpenText(chooser.Filename);

			// Copy the contents into the logTextView
			logTextView.Buffer.Text = file.ReadToEnd();

			// Set the MainWindow Title to the filename.
			this.Title = "Reading Shit Now -- " + chooser.Filename.ToString();

			// Make the MainWindow bigger to accomodate the text in the logTextView
			this.Resize(640, 480);

			// Close the file so as to not leave a mess.
			file.Close();
		}
		
		chooser.Destroy();
	}
	
	protected virtual void OnSave(object sender, System.EventArgs e)
	{
		// Reset the logTreeView and change the window back to original size
		int width, height;
		this.GetDefaultSize(out width, out height);
		this.Resize( width, height );

		// Create and display a fileChooserDialog to save the file
		FileChooserDialog chooser = new FileChooserDialog(
			"Where would you like to save the file...",
			this,
			FileChooserAction.Save,
			"Cancel", ResponseType.Cancel,
			"Save", ResponseType.Accept);

		if (chooser.Run() == (int) ResponseType.Accept)
		{
			// Open the file for reading.
			System.IO.StreamWriter file =
			new System.IO.StreamWriter(chooser.Filename);

			// Copy the contents into the logTextView
			file.Write(logTextView.Buffer.Text);

			// Close the file so as to not leave a mess.
			file.Close();
		}
		
		chooser.Destroy();
    }

    [GLib.ConnectBefore]
	protected virtual void OnKeyPress(object o, Gtk.KeyPressEventArgs args)
	{

        Gdk.Key key = args.Event.Key;

        if (key == Gdk.Key.space || key == Gdk.Key.Left ||
            key == Gdk.Key.Right || key == Gdk.Key.Up ||
            key == Gdk.Key.Down)
        {

            table.Update(0);
        }
	}
}

