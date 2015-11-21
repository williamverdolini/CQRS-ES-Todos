angular.module('TodoApp.settings',[])
    // issue #5401 (https://github.com/angular/angular.js/issues/5401)
    // workaround thanks to http://stackoverflow.com/a/11870341/3316654
    .directive('ngModelOnblur', function() {
        return {
            restrict: 'A',
            require: 'ngModel',
            priority: 1, // needed for angular 1.2.x
            link: function(scope, elm, attr, ngModelCtrl) {
                if (attr.type === 'radio' || attr.type === 'checkbox') return;

                elm.unbind('input').unbind('keydown').unbind('change');
                elm.bind('blur', function() {
                    scope.$apply(function() {
                        ngModelCtrl.$setViewValue(elm.val());
                    });         
                });
            }
        };
    })
    .constant('w$settings', {
        apiUrl: '/api/',
        signalR: {
            NotifierHubName: 'notifierHub',
            logging:true
        }
    })
    .factory('w$utils', [function () {
        var _guid = (function () {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                           .toString(16)
                           .substring(1);
            }
            return function () {
                return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                       s4() + '-' + s4() + s4() + s4();
            };
        })();

        var _getFluentValidationMessage = function (serverErrors) {
            var errorString = new String(serverErrors).replace('Validation failed: \r\n', '');
            var errors = errorString.split(' -- ');
            var message = '<ul>'
            angular.forEach(errors, function (value) { if (value.length) message += '<li>' + value + '</li>' })
            message += '</ul>'
            return message;
        }

        return {
            guid: _guid,
            getFluentValidationMessage: _getFluentValidationMessage
        }
    }])
    .factory('w$signalR', ['$rootScope', 'w$settings', function ($rootScope, w$settings) {
        var broadcastEvent = function (eventName) {
            return function () {
                console.log("event:" + eventName);
                console.log(arguments);
                $rootScope.$broadcast(eventName, arguments.length==1 ? arguments[0] : arguments);
            }
        }

        var notifierHubProxy = $.connection[w$settings.signalR.NotifierHubName];
        // setup of global notification events 
        notifierHubProxy.client.changedToDoListDescription = broadcastEvent('changedToDoListDescription');
        notifierHubProxy.client.createdToDoListEvent = broadcastEvent('createdToDoListEvent');
        notifierHubProxy.client.addedNewToDoItemEvent = broadcastEvent('addedNewToDoItemEvent');
        notifierHubProxy.client.markedToDoItemAsCompletedEvent = broadcastEvent('markedToDoItemAsCompletedEvent');
        notifierHubProxy.client.reOpenedToDoItemEvent = broadcastEvent('reOpenedToDoItemEvent');
        notifierHubProxy.client.changedToDoItemImportanceEvent = broadcastEvent('changedToDoItemImportanceEvent');
        notifierHubProxy.client.changedToDoItemDescriptionEvent = broadcastEvent('changedToDoItemDescriptionEvent');
        notifierHubProxy.client.changedToDoItemDueDateEvent = broadcastEvent('changedToDoItemDueDateEvent');

        //estabish the connection to signalR server
        $.connection.hub.logging = w$settings.signalR.logging;

        return {
            start: function () { $.connection.hub.start();},
            //TODO: is it necessary to stop the connection from client-side?
            stop: function () { $.connection.hub.stop();}
        }
    }])


angular.module('TodoApp', ['TodoApp.settings', 'ngRoute', 'ui.bootstrap', 'ngSanitize']);
angular.module('TodoApp')
    .config(function ($routeProvider) {
        $routeProvider.
            when('/', { controller: 'TodoListController', templateUrl: 'todoList.html' }).
            when('/items/:Id', { controller: 'TodoListItemsController', templateUrl: 'todoItems.html' }).
            otherwise({ redirectTo: "/" });        
    })
    .run(['w$signalR', function (w$signalR) {
        w$signalR.start();
    }])
    .controller('TodoListController', ['$scope', '$http', '$location', 'w$settings', 'w$utils', function ($scope, $http, $location, w$settings, w$utils) {
        $scope.local = {
            TodoListForm: null,
            TodoItemForm: null,
            labels: {
                changeDescription: 'Change the description',
                todoItems: 'Update the tasks'
            },
            list: {
                title: null,
                description: null,
                errors: {
                    show: false,
                    message: ''
                }
            },
            todoList: []
        }

        $scope.actions = {
            addNewList: function () {
                var _input = {
                    id: w$utils.guid(),
                    title: $scope.local.list.title,
                    description: $scope.local.list.description
                }

                $http.post(w$settings.apiUrl + 'ToDoList/CreateNewList', _input)
                    .success(
                        function (result, status, headers) {
                            $scope.local.list.description = null;
                            $scope.local.list.title = null;
                            $scope.local.list.errors.show = false;
                            $scope.local.list.errors.message = null;
                        })
                    .error(
                        function (data, status, headers, config) {
                            $scope.local.list.errors.message = w$utils.getFluentValidationMessage(data.Message);
                            //$scope.local.list.errors.message = new String(data.Message);
                            $scope.local.list.errors.show = true;
                        })
            },
            getTodoList: function () {
                $http.get(w$settings.apiUrl + 'ToDoList/List')
                    .success(
                        function (result, status) {
                            $scope.local.todoList = result;
                        });
            },
            changeDescription: function (_list) {
                var _input = {
                    id: _list.Id,
                    description: _list.Description
                }

                $http.post(w$settings.apiUrl + 'ToDoList/ChangeDescription', _input);
            },
            viewItems: function (_list) {
                $location.path('/items/' + _list.Id)
            },
            migrateLegacyData: function () {
                $http.post(w$settings.apiUrl + 'Migrate')
                    .success(
                        function (result, status) {
                            console.log('Migration successfully created.');
                            $scope.actions.getTodoList();
                        });
            },
            rebuildEvents: function () {
                $http.post(w$settings.apiUrl + 'Rebuild')
                    .success(
                        function (result, status) {
                            console.log('Events Rebuild successfully created.');
                            $scope.actions.getTodoList();
                        });
            }
        }

        $scope.actions.getTodoList();

        //events listeners
        $scope.$on('createdToDoListEvent', function (event, data) {
            console.log('server notified a createdToDoListEvent');
            $scope.$apply(function () {
                $scope.local.todoList.push({
                    Id: data.Id,
                    Title: data.Title,
                    Description: data.Description
                })
            });
        });

        $scope.$on('changedToDoListDescription', function (event, data) {
            console.log('server notified a changedToDoListDescription');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoList, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoList[key].Description = data.Description
                    }
                })
            });
        });
        
    }])
    .controller('TodoListItemsController', ['$scope', '$http', '$location', '$routeParams', 'w$settings', 'w$utils', '$filter', function ($scope, $http, $location, $routeParams, w$settings, w$utils, $filter) {
        $scope.local = {
            item: {
                listId : $routeParams.Id,
                listTitle: null,
                listDescription: null,
                dueDate: null,
                title: null,
                description: null,
                errors: {
                    show: false,
                    message: ''
                },
                dateOptions : {
                    formatYear: 'yy',
                    startingDay: 1
                }

            },
            todoItems: [],
            todoItemsError: {
                show: false,
                message: ''
            }
        }

        var getUniversalDate = function (_date) {
            return new Date(_date.getTime() - (_date.getTimezoneOffset() * 60 * 1000));
        }
        var mapTodoItem = function (element) {
            var item = element;
            item.Status = item.ClosingDate ? 'Closed' : 'Open';
            item.formatDueDate = item.DueDate != null ? new Date(item.DueDate) : null;
            item._Importance = item.Importance;
            item._Description = item.Description;
            item._formatDueDate = item.formatDueDate;
            return item;
        }

        $scope.actions = {
            loadList: function () {
                $http.get(w$settings.apiUrl + 'TodoList/Items/'+$scope.local.item.listId)
                        .success(
                            function (result, status) {
                                $scope.local.todoItems = [];
                                $scope.local.item.listTitle = result.Title;
                                $scope.local.item.listDescription = result.Description;
                                angular.forEach(result.Items, function (element, index) {
                                    this.push(mapTodoItem(element));
                                }, $scope.local.todoItems);
                            });
            },
            open : function($event,todo) {
                $event.preventDefault();
                $event.stopPropagation();
                if (todo) todo.opened = true;
                else $scope.opened = true;
            },
            addNewItem: function () {
                $scope.local.item.errors.show = false;
                $scope.local.item.errors.message = null;

                var _input = {
                    toDoListId: $scope.local.item.listId,
                    id: w$utils.guid(),
                    jsonDueDate: $scope.local.item.dueDate,
                    importance: $scope.local.item.importance,
                    description: $scope.local.item.description,
                    jsonCreationDate: new Date()
                }

                $http.post(w$settings.apiUrl + 'TodoList/Items/' + $scope.local.item.listId + '/Add', _input)
                    .success(
                        function (result, status, headers) {
                            $scope.local.item.description = null;
                            $scope.local.item.importance = null;
                            $scope.local.item.dueDate = null;
                            $scope.local.item.errors.show = false;
                            $scope.local.item.errors.message = null;
                        })
                    .error(
                        function (data, status, headers, config) {
                            $scope.local.item.errors.message = w$utils.getFluentValidationMessage(data.Message);
                            $scope.local.item.errors.show = true;
                        })
            },
            markAsCompleted: function (_item) {
                var _input = {
                    id: _item.Id,
                    jsonClosingDate: new Date()
                }

                $http.post(w$settings.apiUrl + 'TodoItems/MarkAsComplete', _input)
                    .success(function (result, status) {
                        _item.Status = 'Closed';
                        $scope.local.todoItemsError.message = '';
                        $scope.local.todoItemsError.show = false;
                    })
                    .error(function (data, status, headers, config) {
                        $scope.local.todoItemsError.message = w$utils.getFluentValidationMessage(data.Message);
                        $scope.local.todoItemsError.show = true;
                    });
            },
            reOpen: function (_item) {
                var _input = {
                    id: _item.Id
                }

                $http.post(w$settings.apiUrl + 'TodoItems/ReOpen', _input)
                    .success(
                        function (result, status) {
                            _item.Status = 'Open';
                        });
            },
            changeDescription: function (_item) {
                var _input = {
                    id: _item.Id,
                    description: _item.Description
                }

                $http.post(w$settings.apiUrl + 'TodoItems/ChangeDescription', _input)
                    .success(function (result, status) {
                        $scope.local.todoItemsError.message = '';
                        $scope.local.todoItemsError.show = false;
                        _item._Description = _item.Description
                    })
                    .error(function (data, status, headers, config) {
                        $scope.local.todoItemsError.message = w$utils.getFluentValidationMessage(data.Message);
                        $scope.local.todoItemsError.show = true;
                        _item.Description = _item._Description
                    });
            },
            changeDueDate: function (_item) {
                if ((_item.DueDate == null && _item.formatDueDate != null) || (_item.DueDate != null && _item.formatDueDate - new Date(_item.DueDate) != 0)) {
                    _item.DueDate = _item.formatDueDate;
                    var _input = {
                        id: _item.Id,
                        dueDate: getUniversalDate(_item.formatDueDate)
                    }

                    $http.post(w$settings.apiUrl + 'TodoItems/ChangeDueDate', _input)
                        .success(function (result, status) {
                            $scope.local.todoItemsError.message = '';
                            $scope.local.todoItemsError.show = false;
                            _item.DueDate = _item.formatDueDate;
                            _item._formatDueDate = _item.formatDueDate;

                        })
                        .error(function (data, status, headers, config) {
                            $scope.local.todoItemsError.message = w$utils.getFluentValidationMessage(data.Message);
                            $scope.local.todoItemsError.show = true;
                            _item.formatDueDate = _item._formatDueDate;
                        });
                }
            },
            changeImportance: function (_item) {
                var _input = {
                    id: _item.Id,
                    importance : _item.Importance
                }

                $http.post(w$settings.apiUrl + 'TodoItems/ChangeImportance', _input)
                    .success(function (result, status) {
                        $scope.local.todoItemsError.message = '';
                        $scope.local.todoItemsError.show = false;
                        _item._Importance = _item.Importance
                    })
                    .error(function (data, status, headers, config) {
                        $scope.local.todoItemsError.message = w$utils.getFluentValidationMessage(data.Message);
                        $scope.local.todoItemsError.show = true;
                        _item.Importance = _item._Importance
                    });
            }
        }
        $scope.actions.loadList();

        //events listeners
        $scope.$on('addedNewToDoItemEvent', function (event, data) {
            console.log('server notified a addedNewToDoItemEvent');
            $scope.$apply(function () {
                $scope.local.todoItems.push(mapTodoItem(data));
            });
        });
        $scope.$on('markedToDoItemAsCompletedEvent', function (event, data) {
            console.log('server notified a markedToDoItemAsCompletedEvent');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoItems, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoItems[key].ClosingDate = data.ClosingDate;
                        $scope.local.todoItems[key].Status = 'Closed';
                    }
                })
            });
        });
        $scope.$on('reOpenedToDoItemEvent', function (event, data) {
            console.log('server notified a reOpenedToDoItemEvent');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoItems, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoItems[key].ClosingDate = data.ClosingDate;
                        $scope.local.todoItems[key].Status = 'Open';
                    }
                })
            });
        });
        $scope.$on('changedToDoItemImportanceEvent', function (event, data) {
            console.log('server notified a changedToDoItemImportanceEvent');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoItems, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoItems[key]._Importance = data.Importance;
                        $scope.local.todoItems[key].Importance = data.Importance;
                    }
                })
            });
        });
        $scope.$on('changedToDoItemDescriptionEvent', function (event, data) {
            console.log('server notified a changedToDoItemDescriptionEvent');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoItems, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoItems[key]._Description = data.Description;
                        $scope.local.todoItems[key].Description = data.Description;
                    }
                })
            });
        });
        $scope.$on('changedToDoItemDueDateEvent', function (event, data) {
            console.log('server notified a changedToDoItemDueDateEvent');
            $scope.$apply(function () {
                angular.forEach($scope.local.todoItems, function (value, key) {
                    if (value.Id == data.Id) {
                        $scope.local.todoItems[key].DueDate = data.DueDate;
                        $scope.local.todoItems[key].formatDueDate = data.DueDate != null ? new Date(data.DueDate) : null;;
                        $scope.local.todoItems[key]._formatDueDate = $scope.local.todoItems[key].formatDueDate;

                    }
                })
            });
        });               
        // and so on. 
        // Actually also the using of success callback to update the item is a good solution.
        // It deceives the user, but it's fine for features in which error is unlikely.
    }]);