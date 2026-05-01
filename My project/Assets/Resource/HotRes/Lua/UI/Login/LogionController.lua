require("LuaUIMVCBase")
LuaUIMVCBase:subClass("LoginController")

LoginController.Slider
LoginController.Text

function LoginController:New(view,userData)
	self.view = view
    self.model = require("Common.LoadingModel"):New()
    self.userData = userData


    self.Slider = view
end

function LoginController:OnButtonClick(btnName)
	
end

function LoginController:OnShow(loadingText)
	CS.LoadingManager.Instance:OnShowAnimFinished()
end

function LoginController:UpdateProgress(downloadedBytes,totalBytes,DownLoadProgress,msg)
	
end

function LoginController:OnHide(loadingText)
	CS.LoadingManager.Instance:OnHideAnimFinished()
end

function LoginController:OnDestroy()
    self.view = nil
    self.model = nil
end

return LoginController