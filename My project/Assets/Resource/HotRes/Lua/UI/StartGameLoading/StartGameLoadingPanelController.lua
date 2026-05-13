require("LuaUIMVCBase")
LuaUIMVCBase:subClass("StartGameLoadingPanelController")

local UIAutoBind = require("UILoading_AutoBind")

function StartGameLoadingPanelController:New(view,userData)
	self.view = view
    self.model = require("Common.LoadingModel"):New()
    self.userData = userData

    UIAutoBind:AutoBind(view);
    self.bind = UIAutoBind
end

function StartGameLoadingPanelController:OnButtonClick(btnName)
	
end

function StartGameLoadingPanelController:OnShow(loadingText)
	self.bind.loadingText_TextMeshProUGUI.text = loadingText
	CS.LoadingManager.Instance:OnShowAnimFinished()
end

function StartGameLoadingPanelController:UpdateProgress(downloadedBytes,totalBytes,DownLoadProgress,msg)
	self.bind.uILoading_Slider.value = DownLoadProgress
	if DownLoadProgress == 1 then
		self.bind.loadingText_TextMeshProUGUI.text = "等待资源初始化"
	end
end

function StartGameLoadingPanelController:OnHide(loadingText)
	self.bind.loadingText_TextMeshProUGUI.text = loadingText
	CS.LoadingManager.Instance:OnHideAnimFinished()
end

function StartGameLoadingPanelController:OnDestroy()
    self.view = nil
    self.model = nil
end

return StartGameLoadingPanelController