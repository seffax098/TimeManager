using Front.Models;

namespace Front.Services;

public static class DemoDataService
{
    public static List<SiteUsage> CreateDashboardSites()
    {
        var sites = new List<SiteUsage>
        {
            new()
            {
                Name = "youtube.com",
                Minutes = 22,
                Verdict = "отдых",
                Links =
                {
                    new LinkVisit { Url = "https://www.youtube.com/watch?v=example1", Minutes = 12, Verdict = "отдых" },
                    new LinkVisit { Url = "https://www.youtube.com/watch?v=example2", Minutes = 10, Verdict = "отдых" }
                }
            },
            new()
            {
                Name = "tiktok.com",
                Minutes = 7,
                Verdict = "отдых",
                Links =
                {
                    new LinkVisit { Url = "https://www.tiktok.com/@example/video/123", Minutes = 7, Verdict = "отдых" }
                }
            },
            new()
            {
                Name = "instagram.com",
                Minutes = 9,
                Verdict = "отдых",
                Links =
                {
                    new LinkVisit { Url = "https://www.instagram.com/p/example/", Minutes = 4, Verdict = "отдых" },
                    new LinkVisit { Url = "https://www.instagram.com/reel/example/", Minutes = 5, Verdict = "отдых" }
                }
            },
            new()
            {
                Name = "docs.company",
                Minutes = 35,
                Verdict = "работа",
                Links =
                {
                    new LinkVisit { Url = "https://docs.company/project/spec", Minutes = 20, Verdict = "работа" },
                    new LinkVisit { Url = "https://docs.company/project/tasks", Minutes = 15, Verdict = "работа" }
                }
            },
            new()
            {
                Name = "github.com",
                Minutes = 12,
                Verdict = "работа",
                Links =
                {
                    new LinkVisit { Url = "https://github.com/org/repo/pull/10", Minutes = 5, Verdict = "работа" },
                    new LinkVisit { Url = "https://github.com/org/repo/issues/7", Minutes = 7, Verdict = "работа" }
                }
            }
        };

        var maxMinutes = Math.Max(sites.Max(x => x.Minutes), 1);
        foreach (var site in sites)
        {
            site.ChartHeight = Math.Max(10, site.Minutes * 170.0 / maxMinutes);
        }

        return sites;
    }

    public static List<EmployeeUsage> CreateEmployees() =>
        new()
        {
            new EmployeeUsage
            {
                FullName = "Иванов Иван Иванович",
                Sites =
                {
                    new SiteUsage
                    {
                        Name = "youtube.com",
                        Minutes = 35,
                        Verdict = "отдых",
                        Links =
                        {
                            new LinkVisit { Url = "https://www.youtube.com/watch?v=abc", Minutes = 20, Verdict = "отдых" },
                            new LinkVisit { Url = "https://www.youtube.com/watch?v=def", Minutes = 15, Verdict = "отдых" }
                        }
                    },
                    new SiteUsage
                    {
                        Name = "github.com",
                        Minutes = 50,
                        Verdict = "работа",
                        Links =
                        {
                            new LinkVisit { Url = "https://github.com/org/repo/pull/12", Minutes = 25, Verdict = "работа" },
                            new LinkVisit { Url = "https://github.com/org/repo/issues/8", Minutes = 25, Verdict = "работа" }
                        }
                    }
                }
            },
            new EmployeeUsage
            {
                FullName = "Петрова Анна Сергеевна",
                Sites =
                {
                    new SiteUsage
                    {
                        Name = "docs.company",
                        Minutes = 60,
                        Verdict = "работа",
                        Links =
                        {
                            new LinkVisit { Url = "https://docs.company/spec", Minutes = 35, Verdict = "работа" },
                            new LinkVisit { Url = "https://docs.company/tasks", Minutes = 25, Verdict = "работа" }
                        }
                    },
                    new SiteUsage
                    {
                        Name = "instagram.com",
                        Minutes = 15,
                        Verdict = "отдых",
                        Links =
                        {
                            new LinkVisit { Url = "https://www.instagram.com/p/example/", Minutes = 10, Verdict = "отдых" },
                            new LinkVisit { Url = "https://www.instagram.com/reel/example/", Minutes = 5, Verdict = "отдых" }
                        }
                    }
                }
            },
            new EmployeeUsage
            {
                FullName = "Сидоров Максим Олегович",
                Sites =
                {
                    new SiteUsage
                    {
                        Name = "tiktok.com",
                        Minutes = 20,
                        Verdict = "отдых",
                        Links =
                        {
                            new LinkVisit { Url = "https://www.tiktok.com/@ex/video/1", Minutes = 12, Verdict = "отдых" },
                            new LinkVisit { Url = "https://www.tiktok.com/@ex/video/2", Minutes = 8, Verdict = "отдых" }
                        }
                    },
                    new SiteUsage
                    {
                        Name = "jira.company",
                        Minutes = 40,
                        Verdict = "работа",
                        Links =
                        {
                            new LinkVisit { Url = "https://jira.company/browse/PROJ-10", Minutes = 18, Verdict = "работа" },
                            new LinkVisit { Url = "https://jira.company/browse/PROJ-22", Minutes = 22, Verdict = "работа" }
                        }
                    }
                }
            }
        };
}
