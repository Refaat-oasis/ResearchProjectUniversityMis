using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;

namespace researchProject.Repositories.Interfaces
{
    public interface IJudgeRepository
    {
        Task<Judge?> GetProfileByUserIdAsync(string userId);
        Task<List<Evaluation>> GetAssignedEvaluationsAsync(string judgeUserId);
        Task<Evaluation?> GetEvaluationDetailsAsync(int evaluationId, string judgeUserId);
        Task<(bool Succeeded, string? ErrorMessage)> SubmitEvaluationAsync(EvaluationSubmitDto dto, string judgeUserId);
    }
}
