using AHPDecisionMaker.Web.ViewModels;
using AHPDecisionMaker.Web.Entities;

namespace AHPDecisionMaker.Web.Services;

public interface IAhpCalculationService
{
    AhpResultDto Calculate(DecisionModel model);
}
