using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace Risen.WorldGenerator.DataModel
{
    /// <summary>
    /// Base class for <see cref="Zone"/> and <see cref="Room"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class ZoneCommon : Risen.WorldGenerator.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public ZoneCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            _uniqueId = uniqueId;
            _title = title;
            _subtitle = subtitle;
            _description = description;
            _imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return _uniqueId; }
            set { SetProperty(ref _uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private ImageSource _image;
        private String _imagePath;
        public ImageSource Image
        {
            get
            {
                if (_image == null && _imagePath != null)
                {
                    _image = new BitmapImage(new Uri(_baseUri, _imagePath));
                }
                return _image;
            }

            set
            {
                _imagePath = null;
                SetProperty(ref _image, value);
            }
        }

        public void SetImage(String path)
        {
            _image = null;
            _imagePath = path;
            OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class Room : ZoneCommon
    {
        public Room(String uniqueId, String title, String subtitle, String imagePath, String description, String content, Zone group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            _content = content;
            _group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        private Zone _group;
        public Zone Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class Zone : ZoneCommon
    {
        public Zone(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Rooms.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a ZonesPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopRooms.Insert(e.NewStartingIndex,Rooms[e.NewStartingIndex]);
                        if (TopRooms.Count > 12)
                        {
                            TopRooms.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopRooms.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopRooms.RemoveAt(e.OldStartingIndex);
                        TopRooms.Add(Rooms[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopRooms.Insert(e.NewStartingIndex, Rooms[e.NewStartingIndex]);
                        TopRooms.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopRooms.RemoveAt(e.OldStartingIndex);
                        if (Rooms.Count >= 12)
                        {
                            TopRooms.Add(Rooms[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopRooms[e.OldStartingIndex] = Rooms[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopRooms.Clear();
                    while (TopRooms.Count < Rooms.Count && TopRooms.Count < 12)
                    {
                        TopRooms.Add(Rooms[TopRooms.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<Room> _rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _rooms; }
        }

        private ObservableCollection<Room> _topRoom = new ObservableCollection<Room>();
        public ObservableCollection<Room> TopRooms
        {
            get {return _topRoom; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// ZoneDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class ZoneDataSource
    {
        private static readonly ZoneDataSource _zoneDataSource = new ZoneDataSource();

        private readonly ObservableCollection<Zone> _allZones = new ObservableCollection<Zone>();
        
        public ObservableCollection<Zone> AllZones
        {
            get { return _allZones; }
        }

        public static IEnumerable<Zone> GetZones(string uniqueId)
        {
            if (!uniqueId.Equals("AllZones")) throw new ArgumentException("Only 'AllZones' is supported as a collection of groups");
            
            return _zoneDataSource.AllZones;
        }

        public static Zone GetZone(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _zoneDataSource.AllZones.Where(zone => zone.UniqueId.Equals(uniqueId)).ToList();

            return matches.Count() == 1 ? matches.First() : null;
        }

        public static Room GetRoom(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _zoneDataSource.AllZones.SelectMany(zone => zone.Rooms).Where(room => room.UniqueId.Equals(uniqueId)).ToList();

            return matches.Count() == 1 ? matches.First() : null;
        }

        public ZoneDataSource()
        {
            var aldest = new Zone("Aldest", "Aldest Town Zone", "Starting Town", "Assets/LightGray.png", "The room definition of the town Aldest.");
            
            aldest.Rooms.Add(new Room("Aldest-Spawn", "Spawn Room", "Starting Room", "Assets/Town.png",
                                              "The starting room for all players",
                                              "You awaken to find yourself standing amidst a room, dark and cold, with suffocating silence. There is a flickering light somewhere in the distance ahead of you.",
                                              aldest));

            aldest.Rooms.Add(new Room("Aldest-Tutorial-1", "Class Selection", "Choose a class in this room",
                                              "Assets/StoreLogo.png", "Class Choice Room - yay!",
                                              "The room where you choose a class. An explanation of each will be retrievable through dialog with someone, for example.",
                                              aldest));

            aldest.Rooms.Add(new Room("Aldest-Tutorial-2", "Deity Selection", "Choose a deity in this room",
                                              "Assets/StoreLogo.png", "Class Deity Room - yay!",
                                              "The room where you choose a deity. An explanation of each will be retrievable through dialog with someone, for example.",
                                              aldest));

            aldest.Rooms.Add(new Room("Aldest-Tutorial-3", "Race Selection", "Choose a race in this room",
                                              "Assets/StoreLogo.png", "Race Choice Room - yay!",
                                              "The room where you choose a race. An explanation of each will be retrievable through dialog with someone, for example.",
                                              aldest));

            _allZones.Add(aldest);

            var dungeon = new Zone("Pit of Trials", "Pit of Trials Dungeon Zone", "Dungeon", "Assets/Dungeon.png",
                                   "The room definition of the pit of trials.");

            dungeon.Rooms.Add(new Room("PitOfTrials-Spawn", "Dungeon Entrance", "Entrance", "Assets/Dungeon.png",
                                              "The entrance to the pit of trials",
                                              "The air smells of decay, and hangs heavy with the weight of malignant acts committed within its walls from eons past.",
                                              dungeon));
            _allZones.Add(dungeon);
        }
    }
}
