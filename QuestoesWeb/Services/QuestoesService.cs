using System.Text.Json;
using QuestoesWeb.Models;

namespace QuestoesWeb.Services;

public interface IQuestoesService
{
    Task<IReadOnlyList<QuestaoViewModel>> ObterQuestoesAsync(CancellationToken cancellationToken = default);

    Task<QuestaoViewModel?> ObterQuestaoPorOrdemAsync(
        int ordem,
        CancellationToken cancellationToken = default);
}

public sealed class QuestoesService(
    IWebHostEnvironment environment,
    ILogger<QuestoesService> logger)
    : IQuestoesService
{
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ILogger<QuestoesService> _logger = logger;

    private IReadOnlyList<QuestaoViewModel>? _cache;

    public async Task<IReadOnlyList<QuestaoViewModel>> ObterQuestoesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var filePath = Path.Combine(_environment.ContentRootPath, "Questoes.json");

        if (!File.Exists(filePath))
        {
            _logger.LogError("Arquivo de questões não encontrado em {FilePath}", filePath);
            return Array.Empty<QuestaoViewModel>();
        }

        await using var stream = File.OpenRead(filePath);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        var items = await JsonSerializer.DeserializeAsync<List<ProvaItemModel>>(
            stream,
            options,
            cancellationToken) ?? [];

        var questoes = items
            .OrderBy(q => q.Ordem)
            .Select(q =>
                new QuestaoViewModel(
                    q.Ordem,
                    q.Questao.IdQuestao,
                    q.Questao.Enunciado ?? string.Empty,
                    q.Questao.VideoEmbed,
                    q.Questao.Respostas
                        .Select(r => new RespostaViewModel(
                            r.IdResposta,
                            r.Texto ?? string.Empty,
                            r.EhCorreta))
                        .ToList(),
                    RespostaSelecionadaId: null,
                    EstaCorreta: null))
            .ToList();

        _cache = questoes;

        return questoes;
    }

    public async Task<QuestaoViewModel?> ObterQuestaoPorOrdemAsync(
        int ordem,
        CancellationToken cancellationToken = default)
    {
        var questoes = await ObterQuestoesAsync(cancellationToken);

        return questoes.FirstOrDefault(q => q.Ordem == ordem);
    }
}


