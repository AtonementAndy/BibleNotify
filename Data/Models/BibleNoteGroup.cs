using System.Collections.ObjectModel;

namespace BibleNotify.Data.Models;

public sealed class BibleNoteGroup(int bookId, string bookName) : ObservableCollection<BibleNote>
{
    public int BookId { get; } = bookId;

    public string BookName { get; } = bookName;
}
