using System.Text.Json.Serialization;

namespace QuestoesWeb.Models;

// PLAN:
// - Map apenas os campos necessários do JSON enorme para records enxutos.
// - Converter o JSON em uma lista de questões prontas para a View.

public sealed record ProvaItemModel(
    [property: JsonPropertyName("ordem")] int Ordem,
    [property: JsonPropertyName("questao")] QuestaoModel Questao
);

public sealed record QuestaoModel(
    [property: JsonPropertyName("id_questao")] int IdQuestao,
    [property: JsonPropertyName("enunciado")] string? Enunciado,
    [property: JsonPropertyName("video_embed")] string? VideoEmbed,
    [property: JsonPropertyName("respostas")] IReadOnlyList<RespostaModel> Respostas
);

public sealed record RespostaModel(
    [property: JsonPropertyName("id_resposta")] int IdResposta,
    [property: JsonPropertyName("resposta")] string? Texto,
    [property: JsonPropertyName("correta")] int Correta
)
{
    public bool EhCorreta => Correta == 1;
}

// ViewModel usada na tela interativa
public sealed record QuestaoViewModel(
    int Ordem,
    int IdQuestao,
    string Enunciado,
    string? VideoEmbed,
    IReadOnlyList<RespostaViewModel> Respostas,
    int? RespostaSelecionadaId,
    bool? EstaCorreta
);

public sealed record RespostaViewModel(
    int IdResposta,
    string Texto,
    bool EhCorreta
);


