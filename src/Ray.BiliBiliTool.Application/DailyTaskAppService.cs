﻿using System;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Agent.Dtos;
using Ray.BiliBiliTool.Application.Attributes;
using Ray.BiliBiliTool.Application.Contracts;
using Ray.BiliBiliTool.DomainService.Interfaces;

namespace Ray.BiliBiliTool.Application
{
    public class DailyTaskAppService : IDailyTaskAppService
    {
        private readonly ILogger<DailyTaskAppService> _logger;
        private readonly IAccountDomainService _loginDomainService;
        private readonly IVideoDomainService _videoDomainService;
        private readonly IMangaDomainService _mangaDomainService;
        private readonly ILiveDomainService _liveDomainService;
        private readonly IVipPrivilegeDomainService _vipPrivilegeDomainService;
        private readonly IChargeDomainService _chargeDomainService;

        public DailyTaskAppService(
            ILogger<DailyTaskAppService> logger,
            IAccountDomainService loginDomainService,
            IVideoDomainService videoDomainService,
            IMangaDomainService mangaDomainService,
            ILiveDomainService liveDomainService,
            IVipPrivilegeDomainService vipPrivilegeDomainService,
            IChargeDomainService chargeDomainService)
        {
            _logger = logger;
            _loginDomainService = loginDomainService;
            _videoDomainService = videoDomainService;
            _mangaDomainService = mangaDomainService;
            _liveDomainService = liveDomainService;
            _vipPrivilegeDomainService = vipPrivilegeDomainService;
            _chargeDomainService = chargeDomainService;
        }

        public void DoDailyTask()
        {
            UseInfo userInfo;
            DailyTaskInfo dailyTaskInfo;

            userInfo = Login();
            dailyTaskInfo = GetDailyTaskStatus();

            WatchAndShareVideo(dailyTaskInfo);
            AddCoinsForVideo();
            LiveSign();
            userInfo.Money = ExchangeSilver2Coin();
            ReceiveVipPrivilege(userInfo);
            Charge(userInfo);
            MangaSign();
            ReceiveMangaVipReward(userInfo);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [TaskInterceptor("登录")]
        private UseInfo Login()
        {
            UseInfo userInfo = null;
            try
            {
                userInfo = _loginDomainService.LoginByCookie();
            }
            catch (Exception e)
            {
                _logger.LogCritical("登录失败，任务结束。Msg:{msg}\r\n", e.Message);
            }
            if (userInfo == null) throw new Exception("登录失败，请检查Cookie");//终止流程
            return userInfo;
        }

        /// <summary>
        /// 获取任务完成情况
        /// </summary>
        /// <returns></returns>
        [TaskInterceptor(null, false)]
        private DailyTaskInfo GetDailyTaskStatus()
        {
            return _loginDomainService.GetDailyTaskStatus();
        }

        /// <summary>
        /// 观看、分享视频
        /// </summary>
        [TaskInterceptor("观看、分享视频", false)]
        private void WatchAndShareVideo(DailyTaskInfo dailyTaskInfo)
        {
            _videoDomainService.WatchAndShareVideo(dailyTaskInfo);
        }

        /// <summary>
        /// 投币任务
        /// </summary>
        [TaskInterceptor("投币任务", false)]
        private void AddCoinsForVideo()
        {
            _videoDomainService.AddCoinsForVideo();
        }

        /// <summary>
        /// 直播中心签到
        /// </summary>
        [TaskInterceptor("直播中心签到", false)]
        private void LiveSign()
        {
            _liveDomainService.LiveSign();
        }

        /// <summary>
        /// 直播中心的银瓜子兑换硬币
        /// </summary>
        [TaskInterceptor("直播中心的银瓜子兑换硬币", false)]
        private decimal ExchangeSilver2Coin()
        {
            return _liveDomainService.ExchangeSilver2Coin();
        }

        /// <summary>
        /// 月初领取大会员福利
        /// </summary>
        [TaskInterceptor("月初领取大会员福利", false)]
        private void ReceiveVipPrivilege(UseInfo userInfo)
        {
            _vipPrivilegeDomainService.ReceiveVipPrivilege(userInfo);
        }

        /// <summary>
        /// 月底充电
        /// </summary>
        [TaskInterceptor("充电",false)]
        private void Charge(UseInfo userInfo)
        {
            _chargeDomainService.Charge(userInfo);
        }

        /// <summary>
        /// 漫画签到
        /// </summary>
        [TaskInterceptor("漫画签到", false)]
        private void MangaSign()
        {
            _mangaDomainService.MangaSign();
        }

        /// <summary>
        /// 获取每月大会员漫画权益
        /// </summary>
        [TaskInterceptor("获取每月大会员漫画权益", false)]
        private void ReceiveMangaVipReward(UseInfo userInfo)
        {
            _mangaDomainService.ReceiveMangaVipReward(1, userInfo);
        }
    }
}
