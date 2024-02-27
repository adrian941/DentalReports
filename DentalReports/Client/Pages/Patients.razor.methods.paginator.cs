using DentalReports.Shared.DisplayModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DentalReports.Client.Pages;

public partial class Patients
{


    public void PaginatorChanged()
    {
        if (paginator_EntriesPerPage == 0)
        {
            paginator_EntriesPerPage = 1;
        }
        paginator_TotalRows = FilteredPatients.Count;
        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                              + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);

        // set all to local storage
        _localStorage.SetItemAsync<int>("paginator_EntriesPerPage", paginator_EntriesPerPage);
        _localStorage.SetItemAsync<int>("paginator_CurrentPage", paginator_CurrentPage);




        FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();
        paginator_StringRange = $"{(paginator_CurrentPage - 1) * paginator_EntriesPerPage + 1} to {Math.Min(paginator_CurrentPage * paginator_EntriesPerPage, paginator_TotalRows)}";

        selectedRow = -1;

        _jsRuntime.InvokeVoidAsync("adjustFooterPosition");

        StateHasChanged();

    }

    public async Task InitPaginatorAsync()
    {
        paginator_TotalRows = FilteredPatients.Count;

        try
        {
            paginator_EntriesPerPage = await _localStorage.GetItemAsync<int>("paginator_EntriesPerPage");
            if (paginator_EntriesPerPage < 1) { paginator_EntriesPerPage = Math.Min(10, paginator_TotalRows); }
        }
        catch (Exception)
        {
            paginator_EntriesPerPage = Math.Min(10, paginator_TotalRows);
        }
        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                         + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);


        try
        {
            paginator_CurrentPage = await _localStorage.GetItemAsync<int>("paginator_CurrentPage");
            if (paginator_CurrentPage < 1) { paginator_CurrentPage = 1; }
        }
        catch (Exception)
        {
            paginator_CurrentPage = 1;
        }

        FilteredPatients = FilteredPatients.OrderByDescending(x => x.DateAdded).ToList();
        FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();

        paginator_StringRange = $"{(paginator_CurrentPage - 1) * paginator_EntriesPerPage + 1} to {Math.Min(paginator_CurrentPage * paginator_EntriesPerPage, paginator_TotalRows)}";

    }

    public void PaginatorFirst()
    {
        if (paginator_EntriesPerPage == 0) { paginator_EntriesPerPage = 10; }
        if (paginator_CurrentPage > paginator_PagesNumber)
        {
            paginator_CurrentPage = 1;
        }


        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                           + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);
        paginator_CurrentPage = 1;
        FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();
        PaginatorChanged();
    }
    public void PaginatorPrevious()
    {
        if (paginator_EntriesPerPage == 0) { paginator_EntriesPerPage = 10; }
        if (paginator_CurrentPage > paginator_PagesNumber)
        {
            paginator_CurrentPage = 1;
        }

        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                           + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);
        if (paginator_CurrentPage > 1)
        {
            paginator_CurrentPage--;
            FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();
        }
        PaginatorChanged();
    }
    public void PaginatorNext()
    {
        if (paginator_EntriesPerPage == 0) { paginator_EntriesPerPage = 10; }
        if (paginator_CurrentPage > paginator_PagesNumber)
        {
            paginator_CurrentPage = 1;
        }

        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                           + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);
        if (paginator_CurrentPage < paginator_PagesNumber)
        {
            paginator_CurrentPage++;
            FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();
        }
        PaginatorChanged();
    }
    public void PaginatorLast()
    {
        if (paginator_EntriesPerPage == 0) { paginator_EntriesPerPage = 10; }
        if (paginator_CurrentPage > paginator_PagesNumber)
        {
            paginator_CurrentPage = 1;
        }

        paginator_PagesNumber = paginator_TotalRows / paginator_EntriesPerPage
                           + (paginator_TotalRows % paginator_EntriesPerPage == 0 ? 0 : 1);
        paginator_CurrentPage = paginator_PagesNumber;
        FrontPatients = FilteredPatients.Skip((paginator_CurrentPage - 1) * paginator_EntriesPerPage).Take(paginator_EntriesPerPage).ToList();
        PaginatorChanged();
    }


    private async Task HandleInputChange_Entries(ChangeEventArgs args)
    {
        try
        {
            int inputValue = Convert.ToInt32(args.Value);
            if (inputValue < 1)
            {
                inputValue = 1;
            }
            // if (inputValue > paginator_TotalRows)
            // {
            //     inputValue = paginator_TotalRows;
            // }
            paginator_EntriesPerPage = inputValue;
            paginator_CurrentPage = 1;

            await _localStorage.SetItemAsync<int>("paginator_EntriesPerPage", paginator_EntriesPerPage);
            PaginatorChanged();
        }
        catch (Exception)
        {

        }
    }


    private async Task HandleInputChange_Pages(ChangeEventArgs args)
    {
        try
        {
            int inputValue = Convert.ToInt32(args.Value);
            if (inputValue < 1)
            {
                inputValue = 1;
            }
            if (inputValue > paginator_PagesNumber)
            {
                inputValue = paginator_PagesNumber;
            }
            paginator_CurrentPage = inputValue;
            await _localStorage.SetItemAsync<int>("paginator_CurrentPage", paginator_CurrentPage);
            PaginatorChanged();
        }
        catch (Exception)
        {

        }
    }

}
