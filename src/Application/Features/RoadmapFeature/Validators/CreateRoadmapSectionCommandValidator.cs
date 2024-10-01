using Application.Common.Models;

namespace Application.Features.RoadmapFeature.Validators;

public class CreateRoadmapSectionCommandValidator : AbstractValidator<RoadMapSectionCreateRequestModel>
{
    public CreateRoadmapSectionCommandValidator()
    {
        RuleFor(v => v.RoadmapName)
            .NotEmpty().WithMessage("SectionName is required.")
            .MinimumLength(2).WithMessage("SectionName must at least 2 character")
            .MaximumLength(150).WithMessage("SectionName must not exceed 150 characters.");
        
        RuleFor(v => v.RoadmapDescription)
            .NotEmpty().WithMessage("SectionDescription is required.")
            .MinimumLength(2).WithMessage("SectionDescription must at least 2 character")
            .MaximumLength(150).WithMessage("SectionDescription must not exceed 150 characters.");

        RuleFor(v => v.Edges)
            .NotEmpty();
        
        RuleFor(v => v.ContentJson)
            .NotEmpty();
        
        RuleFor(v => v.RoadmapSubjectIds)
            .NotEmpty();
        
        RuleFor(v => v.TypeExam)
            .NotEmpty();
        
        RuleFor(v => v.Nodes)
            .NotEmpty();
    }
}
