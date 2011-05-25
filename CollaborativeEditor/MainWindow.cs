using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();
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
//        this.logTextView.Buffer.Text += args.Event.Key;
	}
}

