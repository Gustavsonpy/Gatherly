using API.DTOs;
using API.Enums;

namespace API.Services
{
    public class ReportReasonService
    {
        public IEnumerable<ReportReasonDTO> GetAll()
        {
            return new List<ReportReasonDTO>
            {
                new()
                {
                    Id = (int)ReportReason.InappropriateBehavior,
                    Name = "Comportamento inadequado",
                    Description = "Assédio, discurso de ódio, intimidação, etc.",
                    Icon = "sad-face"
                },
                new()
                {
                    Id = (int)ReportReason.InappropriateContent,
                    Name = "Conteúdo inadequado",
                    Description = "Conteúdos ofensivos ou impróprios.",
                    Icon = "x-circle"
                },
                new()
                {
                    Id = (int)ReportReason.Spam,
                    Name = "Spam",
                    Description = "Mensagens ou publicações repetitivas.",
                    Icon = "message-spam"
                },
                new()
                {
                    Id = (int)ReportReason.FalseInformation,
                    Name = "Informação falsa",
                    Description = "Informações enganosas ou falsas.",
                    Icon = "sad-mask"
                },
                new()
                {
                    Id = (int)ReportReason.Other,
                    Name = "Outro",
                    Description = "Outro motivo não listado.",
                    Icon = "x-circle"
                }
            };
        }
    }
}