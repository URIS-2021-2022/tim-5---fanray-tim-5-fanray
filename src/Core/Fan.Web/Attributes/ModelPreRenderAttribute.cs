﻿using Fan.Blog.Models.View;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fan.Web.Attributes
{
    /// <summary>
    /// Raises the <see cref="ModelPreRender{T}"/> event.
    /// </summary>
    public class ModelPreRenderAttribute : TypeFilterAttribute
    {
        public ModelPreRenderAttribute() : base(typeof(ModelPreRenderFilter))
        {
        }
    }

    public class ModelPreRenderFilter : IActionFilter
    {
        private readonly IMediator mediator;

        public ModelPreRenderFilter(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Raises event after action executed but before view renders.
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Controller is Controller controller)
            {
                if (controller.ViewData.Model is PageVM pageVM)
                {
                    mediator.OnModelPreRender(pageVM);
                }

                if (controller.ViewData.Model is BlogPostVM model)
                {
                    mediator.OnModelPreRender(model);
                }

                if (controller.ViewData.Model is BlogPostListVM list)
                {
                    mediator.OnModelPreRender(list);
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
