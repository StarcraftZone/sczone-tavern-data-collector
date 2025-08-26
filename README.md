# 星际战区-酒馆数据采集器

最新版下载地址：https://github.com/StarcraftZone/sczone-tavern-data-collector/releases/latest

## 采集原理
星际酒馆会将比赛次数、积分等信息存储至本地，本工具从本地保存的文件中读取数据并上报至服务器。

请注意，服务端已进行数据有效性校验，校验未通过的数据将会置零。请勿随意修改本地存档的数据。

## 采集频率
首次打开会主动上传，后续在检测到文件变化后会自动上传。若日志中看到错误，可过段时间点击手动上传。

## 排行榜
https://starcraft.zone/tavern

## QQ讨论群
2599679

## 环境依赖
需要 .net framework 4.6，Win10 已经自带，Win10 以下的系统，需要手动安装
