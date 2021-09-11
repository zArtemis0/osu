using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    /// WIP
    /// </summary>
    public class KdeStatusTrayService : IKdeStatusNotifierItem
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/StatusNotifierItem");

        public Action WindowRaise { get; set; }

        private Task<string> returnEmptyString => Task.FromResult(string.Empty);

        private readonly StatusNotifierItemProperties properties = new StatusNotifierItemProperties();

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties.Get(prop));

        public Task<StatusNotifierItemProperties> GetAllAsync()
            => Task.FromResult(properties);

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("暂时不能修改");
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        //Extensions
        public Task ContextMenuAsync(int X, int Y)
        {
            return Task.CompletedTask;
        }

        public Task ActivateAsync(int X, int Y)
        {
            WindowRaise?.Invoke();
            return Task.CompletedTask;
        }

        public Task SecondaryActivateAsync(int X, int Y)
        {
            WindowRaise?.Invoke();
            return Task.CompletedTask;
        }

        public Task ScrollAsync(int Delta, string Orientation)
        {
            return Task.CompletedTask;
        }

        public event Action OnTitleChanged;

        public Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnTitleChanged), handler);

        public event Action OnIconChanged;

        public Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnIconChanged), handler);

        public event Action OnNewAttentionIcon;

        public Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnNewAttentionIcon), handler);

        public event Action OnOverlayIconChanged;

        public Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnOverlayIconChanged), handler);

        public event Action OnMenuCreated;

        public Task<IDisposable> WatchNewMenuAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnMenuCreated), handler);

        public event Action OnTooltipChanged;

        public Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnTooltipChanged), handler);

        public event Action<string> OnStatusChanged;

        public Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnStatusChanged), handler);

        public Task<string> GetCategoryAsync()
            => Task.FromResult("ApplicationStatus");

        public Task<string> GetIdAsync()
            => Task.FromResult("mfosu");

        public Task<string> GetTitleAsync()
            => Task.FromResult("mfosu");

        public Task<string> GetStatusAsync()
            => Task.FromResult("Active");

        public Task<int> GetWindowIdAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIconThemePathAsync()
            => returnEmptyString;

        public Task<ObjectPath> GetMenuAsync()
            => Task.FromResult(this.ObjectPath);

        public Task<bool> GetItemIsMenuAsync()
            => Task.FromResult(false);

        public Task<string> GetIconNameAsync()
            => Task.FromResult("osu");

        public Task<(int, int, byte[])[]> GetIconPixmapAsync()
            => Task.FromResult(Array.Empty<(int, int, byte[])>());

        public Task<string> GetOverlayIconNameAsync()
            => Task.FromResult("osu");

        public Task<(int, int, byte[])[]> GetOverlayIconPixmapAsync()
            => Task.FromResult(Array.Empty<(int, int, byte[])>());

        public Task<string> GetAttentionIconNameAsync()
            => returnEmptyString;

        public Task<(int, int, byte[])[]> GetAttentionIconPixmapAsync()
            => Task.FromResult(Array.Empty<(int, int, byte[])>());

        public Task<string> GetAttentionMovieNameAsync()
            => returnEmptyString;

        public Task<(string, (int, int, byte[])[], string, string)> GetToolTipAsync()
        {
            var abab = (string.Empty, Array.Empty<(int, int, byte[])>(), string.Empty, string.Empty);
            return Task.FromResult(abab);
        }
    }
}
