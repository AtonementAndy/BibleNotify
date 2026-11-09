namespace BibleNotify.Data.Models;

public sealed record BookInfo(
    int BookId,
    string Code,
    string Name,
    string Testament,
    int BookOrder,
    int ChapterCount);
