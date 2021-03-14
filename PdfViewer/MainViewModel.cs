using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using DevExpress.Export.Xl;
using DevExpress.Mvvm;
using DevExpress.Pdf;
using DevExpress.Xpf.PdfViewer;
using PdfViewer.Model;

namespace PdfViewer
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        protected DictionaryManager DictionaryManager;

        protected RecentFileCollection RecentFileCollection;

        #endregion

        #region Services

        protected virtual IMessageBoxService MessageBoxService { get { return null; } }

        protected virtual ISaveFileDialogService SaveFileDialogService { get { return null; } }

        #endregion

        #region Properties

        public virtual bool ShowDefinitions { get; set; }

        public virtual bool SortDefinitionsAscending { get; set; }

        public virtual bool SortDefinitionsDescending { get; set; }

        public virtual int CurrentPageNumber { get; set; }

        public virtual string CurrentDocumentPath { get; set; }

        public virtual CultureInfo InputCulture { get; set; }

        public virtual CultureInfo OutputCulture { get; set; }

        public virtual List<CultureInfo> SupportedLanguages { get; set; }

        public virtual ObservableCollection<RecentFileViewModel> RecentFiles { get; set; }

        public virtual ObservableCollection<DictionaryBookmark> DictionaryBookmarks { get; set; }

        public virtual ICollectionView DictionaryBookmarksView { get; set; }

        #endregion

        #region Commands

        public void ChangeInputCulture(CultureInfo cultureInfo)
        {
            InputCulture = cultureInfo;
        }

        public void ChangeOutputCulture(CultureInfo cultureInfo)
        {
            OutputCulture = cultureInfo;
        }

        public void SwapLanguages()
        {
            CultureInfo inputCulture = InputCulture;

            InputCulture = OutputCulture;
            OutputCulture = inputCulture;
        }

        public async Task Pronounce(string word)
        {
            string url = await DictionaryManager.Pronounce(word, InputCulture);

            if (!String.IsNullOrEmpty(url))
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(url));
                player.Play();

                bool isOpened = false;
                player.MediaOpened += (s, e) => { isOpened = true; };

                while (!isOpened)
                    await Task.Delay(100);

                await Task.Delay(player.NaturalDuration.TimeSpan);
            }
        }

        public bool CanExportToExcel()
        {
            return DictionaryBookmarks.Count > 0;
        }

        public void ExportToExcel()
        {
            SaveFileDialogService.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (SaveFileDialogService.ShowDialog())
            {
                IXlExporter exporter = XlExport.CreateExporter(XlDocumentFormat.Xlsx);
                string[] columnNames = new string[] { "Word", "Definition" };
                string fileName = SaveFileDialogService.GetFullFileName();

                using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (IXlDocument document = exporter.CreateDocument(stream))
                    {
                        using (IXlSheet sheet = document.CreateSheet())
                        {
                            sheet.Name = "Dictionary";

                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInCharacters = 25;
                                column.Formatting = new XlCellFormatting();
                                column.Formatting.Alignment = new XlCellAlignment();
                                column.Formatting.Alignment.VerticalAlignment = XlVerticalAlignment.Center;
                            }

                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInCharacters = 100;
                                column.Formatting = new XlCellFormatting();
                                column.Formatting.Alignment = new XlCellAlignment();
                                column.Formatting.Alignment.WrapText = true;
                            }

                            IXlTable table;
                            using (IXlRow row = sheet.CreateRow())
                                table = row.BeginTable(columnNames, true);

                            foreach (DictionaryBookmark bookmark in DictionaryBookmarksView)
                            {
                                using (IXlRow row = sheet.CreateRow())
                                    row.BulkCells(new string[] { bookmark.Word, bookmark.Definition }, null);
                            }

                            using (IXlRow row = sheet.CreateRow())
                                row.EndTable(table, false);

                            table.Style.ShowFirstColumn = true;
                        }
                    }
                }

                Process.Start(fileName);
            }
        }

        public bool CanSortBookmarks(string parameter)
        {
            return DictionaryBookmarks.Count > 1;
        }

        public void SortBookmarks(string parameter)
        {
            DictionaryBookmarksView.SortDescriptions.Clear();

            if (parameter != null)
            {
                if (Enum.TryParse(parameter, out ListSortDirection direction))
                    DictionaryBookmarksView.SortDescriptions.Add(new SortDescription("Word", direction));
            }
        }

        public void SaveRecentFile(string filePath)
        {
            RecentFile recentFile = RecentFileCollection.FirstOrDefault(x => x.FilePath == filePath);
            if (recentFile != null)
                RecentFileCollection.Remove(recentFile);

            RecentFileCollection.Add(new RecentFile
            {
                FilePath = filePath,
                PageNumber = CurrentPageNumber,
                InputLanguage = InputCulture.Name,
                OutputLanguage = OutputCulture.Name
            });

            RecentFileCollection.SaveToFile();
        }

        #endregion

        #region EventCommands

        public void LoadDocument(IPdfDocument document)
        {
            ShowDefinitions = true;

            RecentFile recentFile = RecentFileCollection.FirstOrDefault(x => x.FilePath == CurrentDocumentPath);
            if (recentFile != null)
            {
                if (recentFile.PageNumber > 0)
                    CurrentPageNumber = recentFile.PageNumber;

                if (!String.IsNullOrEmpty(recentFile.InputLanguage))
                    InputCulture = new CultureInfo(recentFile.InputLanguage);

                if (!String.IsNullOrEmpty(recentFile.InputLanguage))
                    OutputCulture = new CultureInfo(recentFile.OutputLanguage);
            }

            foreach (IPdfPage page in document.Pages)
            {
                if (page is PdfPageViewModel viewModel)
                {
                    foreach (PdfAnnotation annotation in viewModel.Page.Annotations)
                    {
                        if (annotation is PdfTextMarkupAnnotation textMarkupAnnotation)
                        {
                            if (textMarkupAnnotation?.Name.StartsWith(DictionaryBookmark.DictionaryEntry) == false)
                                continue;

                            DictionaryBookmarks.Add(new DictionaryBookmark
                            {
                                PageNumber = page.PageNumber,
                                Name = textMarkupAnnotation.Name,
                                Word = textMarkupAnnotation.Title,
                                WordClass = textMarkupAnnotation.Subject,
                                Definition = textMarkupAnnotation.Contents
                            });
                        }
                    }
                }
            }
        }

        public void SaveDocument(IPdfDocument document)
        {
            if (document != null)
            {
                PdfDocumentViewModel pdfDocument = document as PdfDocumentViewModel;
                if (pdfDocument.IsDocumentModified)
                    pdfDocument.SaveDocument(pdfDocument.FilePath, true, new PdfSaveOptions());

                SaveRecentFile(pdfDocument.FilePath);
            }
        }

        public void CloseDocument(DocumentClosingEventArgs e)
        {
            e.SaveDialogResult = MessageBoxResult.No;
            e.Handled = true;

            DictionaryBookmarks.Clear();
            SaveRecentFile(CurrentDocumentPath);
        }

        public void CreateMarkup(PdfAnnotationCreatingEventArgs e)
        {
            if (e.Builder.AnnotationType == PdfAnnotationType.TextMarkup)
            {
                IPdfViewerTextMarkupAnnotationBuilder annotationBuilder = e.Builder.AsTextMarkupAnnotationBuilder();

                if (String.IsNullOrWhiteSpace(annotationBuilder.SelectedText))
                {
                    e.Cancel = true;
                    e.Handled = true;
                    return;
                }

                SearchResult[] result = AsyncHelper.RunSync(() => DictionaryManager.Search(annotationBuilder.SelectedText, InputCulture, OutputCulture));
                if (result == null || result.Length == 0)
                {
                    e.Cancel = true;
                    e.Handled = true;
                    return;
                }

                annotationBuilder.Name = String.Format("{0}-{1}", DictionaryBookmark.DictionaryEntry, annotationBuilder.Name);
                annotationBuilder.Author = result[0].Word;
                annotationBuilder.Subject = result[0].WordClass;
                annotationBuilder.Contents = result[0].Definition;            

                DictionaryBookmarks.Add(new DictionaryBookmark
                {
                    PageNumber = annotationBuilder.PageNumber,
                    Name = annotationBuilder.Name,
                    Word = result[0].Word,
                    WordClass = result[0].WordClass,
                    Definition = result[0].Definition
                });

                Cursor.Position = new System.Drawing.Point(Cursor.Position.X - 1, Cursor.Position.Y - 1);
            }
        }

        public void DeleteMarkup(PdfAnnotationDeletingEventArgs e)
        {
            if (MessageBoxService.ShowMessage("Do you want to delete selected item?", "Confirm Delete", MessageButton.YesNo, MessageIcon.Question) == MessageResult.No)
            {
                e.Cancel = true;
                return;
            }

            DictionaryBookmark bookmark = DictionaryBookmarks.FirstOrDefault(x => x.Name == e.Annotation.Name);
            if (bookmark != null)
                DictionaryBookmarks.Remove(bookmark);
        }

        public void SelectDictionaryEntry(PdfAnnotationGotFocusEventArgs e)
        {
            if (e.Annotation.Name.StartsWith(DictionaryBookmark.DictionaryEntry))
            {
                DictionaryBookmark bookmark = DictionaryBookmarks.FirstOrDefault(x => x.Name == e.Annotation.Name);
                if (bookmark != null)
                    DictionaryBookmarksView.MoveCurrentTo(bookmark);
            }
        }

        #endregion

        public MainViewModel()
        {
            if (!IsInDesignMode)
                DictionaryManager = new DictionaryManager("Dictionary.db");

            InputCulture = new CultureInfo("en");
            OutputCulture = new CultureInfo("tr");

            SupportedLanguages = new List<CultureInfo>()
            {
                new CultureInfo("en"),
                new CultureInfo("tr"),
                new CultureInfo("de"),
                new CultureInfo("fr"),
                new CultureInfo("es"),
                new CultureInfo("it"),
            };
            
            CurrentPageNumber = 1;
            DictionaryBookmarks = new ObservableCollection<DictionaryBookmark>();
            DictionaryBookmarksView = CollectionViewSource.GetDefaultView(DictionaryBookmarks);

            RecentFiles = new ObservableCollection<RecentFileViewModel>();
            RecentFileCollection = RecentFileCollection.LoadFromFile();
            foreach (RecentFile recentFile in RecentFileCollection)
            {
                if (File.Exists(recentFile.FilePath))
                {
                    RecentFiles.Add(new RecentFileViewModel
                    {
                        DocumentSource = recentFile.FilePath,
                        Name = Path.GetFileName(recentFile.FilePath),
                        Command = new DelegateCommand<String>(x => CurrentDocumentPath = x)
                    });
                }
            }
        }

        protected virtual void OnSortDefinitionsAscendingChanged()
        {
            if (DictionaryBookmarksView.SortDescriptions.Contains(SortDescendingDescription))
            {
                SortDefinitionsDescending = false;
                DictionaryBookmarksView.SortDescriptions.Remove(SortDescendingDescription);
            }

            if (SortDefinitionsAscending)
                DictionaryBookmarksView.SortDescriptions.Add(SortAscendingDescription);
            else
                DictionaryBookmarksView.SortDescriptions.Remove(SortAscendingDescription);
        }

        protected virtual void OnSortDefinitionsDescendingChanged()
        {
            if (DictionaryBookmarksView.SortDescriptions.Contains(SortAscendingDescription))
            {
                SortDefinitionsAscending = false;
                DictionaryBookmarksView.SortDescriptions.Remove(SortAscendingDescription);
            };
            
            if (SortDefinitionsDescending)
                DictionaryBookmarksView.SortDescriptions.Add(SortDescendingDescription);
            else
                DictionaryBookmarksView.SortDescriptions.Remove(SortDescendingDescription);
        }

        protected SortDescription SortAscendingDescription = new SortDescription("Word", ListSortDirection.Ascending);

        protected SortDescription SortDescendingDescription = new SortDescription("Word", ListSortDirection.Descending);
    }
}