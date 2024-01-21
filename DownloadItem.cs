﻿using AngleSharp.Text;
using Timer = System.Windows.Forms.Timer;

namespace DownloadManager
{
    public class DownloadItem
    {
        CurrentDownloads downloadsForm = CurrentDownloads._instance;
        Timer timer = new Timer();
        DownloadProgress progress;
        int index = 0;
        bool isYtDownload = false;
        bool contentLengthIssue = false;

        public void Initialize(DownloadProgress progress)
        {
            this.progress = progress;

            index = downloadsForm.progressGridView.Rows.Add(progress.fileName, progress.percentageDone, progress.url, "? B");

            timer.Tick += UpdateTimer_Tick;
            timer.Interval = 500;
            timer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (progress == null || progress.downloading == false || progress.cancelled == true)
            {
                // If the download is finished, dispose the item
                Dispose();
                timer.Stop();
                return;
            }
            else
            {
                // Update the values
                try
                {
                    downloadsForm.progressGridView.Rows[index].Cells[0].Value = progress.fileName;
                    downloadsForm.progressGridView.Rows[index].Cells[2].Value = progress.url;
                    if (progress.totalSize < 1)
                    {
                        downloadsForm.progressGridView.Rows[index].Cells[3].Value = "? B";
                    }
                    else
                    {
                        long bytes = progress.totalSize;
                        long kilobytes = (progress.totalSize / 1024);
                        long megabytes = ((progress.totalSize / 1024) / 1024);
                        long gigabytes = (((progress.totalSize / 1024) / 1024) / 1024);

                        if (gigabytes > 1)
                        {
                            downloadsForm.progressGridView.Rows[index].Cells[3].Value = $"{gigabytes} GB";
                        }
                        else if (megabytes > 1)
                        {
                            downloadsForm.progressGridView.Rows[index].Cells[3].Value = $"{megabytes} MB";
                        }
                        else if (kilobytes > 1)
                        {
                            downloadsForm.progressGridView.Rows[index].Cells[3].Value = $"{kilobytes} KB";
                        }
                        else
                        {
                            downloadsForm.progressGridView.Rows[index].Cells[3].Value = $"{bytes} B";
                        }
                    }

                    if (progress.downloadType == DownloadProgress.DownloadType.YoutubePlaylist)
                    {
                        // Update the progressBar
                        downloadsForm.progressGridView.Rows[index].Cells[1].Value = (int)progress.progressLabel.Text.Replace("%", "").ToDouble();
                    }
                    else if (progress.downloadType == DownloadProgress.DownloadType.YoutubeVideo)
                    {
                        downloadsForm.progressGridView.Rows[index].Cells[1].Tag = -1;
                    }
                    else
                    {
                        // Update the progress bar
                        if ((int)progress.percentageDone > 100)
                        {
                            Logging.Log("DownloadItem Progress is greater than 100%! The item has been removed to prevent a crash!", Color.Orange);
                            Dispose();
                        }

                        if (progress.totalSize < 1)
                        {
                            // If the total size is less than 1, then we cannot report progress
                            // Update the progress bar
                            downloadsForm.progressGridView.Rows[index].Cells[1].Tag = -1;
                        }
                        else
                        {
                            // Update the progress bar
                            downloadsForm.progressGridView.Rows[index].Cells[1].Tag = (int)progress.progressLabel.Text.Replace("%", "").ToDouble();
                        }

                        //Logging.Log(((int)progress.progressLabel.Text.Replace("%", "").ToDouble()).ToString(), Color.Gray);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log($"{ex.Message} ({ex.GetType().FullName})\n{ex.StackTrace}", Color.Red);
                }
            }
        }

        public void Dispose()
        {
            downloadsForm.itemList.Remove(this);
            try
            {
                downloadsForm.progressGridView.Rows.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Logging.Log($"{ex.Message} ({ex.GetType().FullName})\n{ex.StackTrace}", Color.Red);
            }

            timer.Stop();
            timer.Dispose();
        }

        public void DisposeNoRemove()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
