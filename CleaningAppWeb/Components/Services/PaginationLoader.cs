using CleaningAppWeb.Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CleaningAppWeb.Components.Services
{
    public abstract class PaginationLoader : ComponentBase, IAsyncDisposable
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = null!;

        private int _isLoadingMoreFlag = 0;
        private bool _isFirstLoad = true;

        protected bool _isLoadingMore = false;
        protected bool _isInitialized = false;

        protected ElementReference _tableContainer;
        protected DotNetObjectReference<ComponentBase>? _dotNetHelper;
        protected IJSObjectReference? _jsModule;

        protected int _currentPage = 1;
        protected int _totalCount = 0;

        private bool HasMoreItems() => GetCurrentItemsCount() < _totalCount;

        private async Task InitializeInfiniteScrollAsync()
        {
            _dotNetHelper = DotNetObjectReference.Create((ComponentBase)this);
            try
            {
                await AdditionalAfterRenderMethod();

                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/infiniteScroll.js");
                await _jsModule.InvokeVoidAsync("initializeInfiniteScroll", _tableContainer, _dotNetHelper);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации дозагрузки данных: {ex.Message}");
            }
        }

        protected async Task LoadDataAsync<T>(
            List<T> collection, Func<Task<ListDataResponse<T>>> method, bool append = false
        )
        {
            if (!append)
            {
                ResetPagination();
                collection.Clear();
            }

            StateHasChanged();

            var listResponse = await method.Invoke();

            _totalCount = listResponse.Count;
            if (listResponse.List.Count > 0)
            {
                if (append)
                    collection.AddRange(listResponse.List);
                else
                {
                    collection.Clear();
                    collection.AddRange(listResponse.List);
                }

                IncrementPage();
            }

            StateHasChanged();

            if (!append && !_isFirstLoad)
                await ReinitializeScrollIfNeededAsync();

            _isFirstLoad = false;
        }

        private async Task LoadMoreItemsInternalAsync()
        {
            _isLoadingMore = true;
            StateHasChanged();

            try
            {
                await OnLoadMoreItemsAsync();
            }
            finally
            {
                _isLoadingMore = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Метод, вызывающий загрузку данных
        /// </summary>
        protected abstract Task OnLoadMoreItemsAsync();

        /// <summary>
        /// Метод для получения количества данных
        /// </summary>
        protected abstract int GetCurrentItemsCount();

        /// <summary>
        /// Метод для дополнительной очистки в наследниках
        /// </summary>
        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

        /// <summary>
        /// Метод для дополнительной регистрации JS
        /// </summary>
        protected virtual async Task AdditionalAfterRenderMethod() { return; }

        /// <summary>
        /// Метод, завершающий инициализацию
        /// </summary>
        protected void MarkAsInitialized()
        {
            _isInitialized = true;
            StateHasChanged();
        }

        /// <summary>
        /// Сброс пагинации
        /// </summary>
        protected void ResetPagination()
        {
            _currentPage = 1;
            _totalCount = 0;
        }

        /// <summary>
        /// Переход на другую страницу
        /// </summary>
        protected void IncrementPage()
        {
            if (HasMoreItems()) ++_currentPage;
        }

        /// <summary>
        /// Перегрузка метода ComponentBase
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Task.Delay(100);
                await InitializeInfiniteScrollAsync();
            }
        }

        /// <summary>
        /// Завершение дозагрузки данных
        /// </summary>
        protected async Task StopInfiniteScrollAsync()
        {
            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("stopInfiniteScroll", _tableContainer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка остановки скролла: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Переинициализирует Infinite Scroll (вызывать после сброса данных: поиск, фильтры)
        /// </summary>
        protected async Task ReinitializeScrollIfNeededAsync()
        {
            if (!HasMoreItems()) return;

            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("initializeInfiniteScroll", _tableContainer, _dotNetHelper);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка переинициализации скролла: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Загрузка данных, вызывается с помощью JS
        /// </summary>
        [JSInvokable]
        public async Task LoadMoreItems()
        {
            if (Interlocked.CompareExchange(ref _isLoadingMoreFlag, 1, 0) != 0)
                return;

            try
            {
                if (!HasMoreItems())
                {
                    await StopInfiniteScrollAsync();
                    return;
                }
                await LoadMoreItemsInternalAsync();
            }
            finally
            {
                Interlocked.Exchange(ref _isLoadingMoreFlag, 0);
            }
        }

        /// <summary>
        /// Выгрузка зависимостей
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            _dotNetHelper?.Dispose();

            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("stopInfiniteScroll", _tableContainer);
                    await _jsModule.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при освобождении JS модуля: {ex.Message}");
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
