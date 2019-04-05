
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTagger
{

    /// <summary>
    /// Interaction logic for EditMyTags.xaml
    /// </summary>
    public partial class EditMyTags : Window
    {
        public EditMyTags()
        {
            InitializeComponent();
            /*
            tabcontrol.ItemsSource = new List<object>(new object[]
            {
                new {
                        Category = "bugTypes",
                        Tags = new List<object>(new object[]{ new {TagName = "b1u1g1s1" }, new {TagName = "bu2g2s2" }, new {TagName = "bu3g3s" }, })
                },
                new {
                        Category = "fugTypes",
                        Tags = new List<object>(new object[]{ new {TagName = "f1u1g1s1" }, new {TagName = "fu2g2s2" }, new {TagName = "fu3g3s" }, })
                },
                new {
                        Category = "lugTypes",
                        Tags = new List<object>(new object[]{ new {TagName = "l1u1g1s1" }, new {TagName = "lu2g2s2" }, new {TagName = "lu3g3s" }, })
                }
            });
            */
            tabcontrol.ItemsSource = new List<TagsRecordViewModel>(new TagsRecordViewModel[]{
                new TagsRecordViewModel("giif", new ObservableCollection<ImageTag>(new ImageTag[]{ new ImageTag("test1"),new ImageTag("test6"),new ImageTag("test2"), })),
                new TagsRecordViewModel("gfgfhgj", new ObservableCollection<ImageTag>(new ImageTag[]{ new ImageTag("jhghfg"),new ImageTag("hgfd"),new ImageTag("sdftg"), })),
                new TagsRecordViewModel("sdfg", new ObservableCollection<ImageTag>(new ImageTag[]{ new ImageTag("5467"),new ImageTag("654"),new ImageTag("78965"), })),
            });

        }
    }

}

namespace ImageTagger.DataModels
{
    public class TagsRecordViewModel
    {
        public TagsRecordViewModel(string category, ObservableCollection<ImageTag> tags)
        {
            Category = category;
            Tags = tags;
        }

        public string Category { get; set; } = "";
        public ObservableCollection<ImageTag> Tags { get; } = new ObservableCollection<ImageTag>();
    }
}